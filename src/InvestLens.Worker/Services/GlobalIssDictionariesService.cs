using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Messages;

namespace InvestLens.Worker.Services;

public class GlobalIssDictionariesService : IGlobalIssDictionariesService
{
    private readonly IMessageBusClient _messageBusClient;
    private readonly IPollyService _pollyService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<GlobalIssDictionariesService> _logger;
    private readonly CancellationToken _cancellationToken;

    public GlobalIssDictionariesService(IMessageBusClient messageBusClient, IPollyService pollyService,
        ICorrelationIdService correlationIdService, ILogger<GlobalIssDictionariesService> logger)
    {
        _messageBusClient = messageBusClient;
        _pollyService = pollyService;
        _correlationIdService = correlationIdService;
        _logger = logger;
        _cancellationToken = CancellationToken.None;
    }

    public async Task ProcessDailyDataRefreshAsync()
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId("daily");
        CorrelationHelper.CallLogWithCorrelationId(correlationId,
            () => _logger.LogInformation(
                "Launching a daily update of the securities list. CorrelationId: {CorrelationId}", correlationId));
        try
        {
            await GlobalIssDictionariesSecuritiesAsync(correlationId);
            CorrelationHelper.CallLogWithCorrelationId(correlationId,
                () => _logger.LogInformation(
                    "Daily updating of the securities list has been completed successfully. CorrelationId: {CorrelationId}",
                    correlationId));
        }
        catch (Exception ex)
        {
            CorrelationHelper.CallLogWithCorrelationId(correlationId,
                () => _logger.LogError(ex,
                    "The daily update of the securities list ended with an error. CorrelationId: {CorrelationId}",
                    correlationId));
            throw;
        }
    }

    public async Task InitializeApplicationAsync()
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId("init");
        await CorrelationHelper.CallLogWithCorrelationIdAsync(correlationId, async () =>
        {
            _logger.LogInformation("Launching an update of the securities list during application initialization.");
            try
            {
                await GlobalIssDictionariesSecuritiesAsync(correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The launch and updating of the securities list failed during application initialization.");
                throw;
            }
        });
    }

    #region Private Methods

    private async Task GlobalIssDictionariesSecuritiesAsync(string correlationId)
    {
        var resilientPolicy = _pollyService.GetRabbitMqResilientPolicy();
        var message = new GlobalIssDictionariesRefreshMessage();
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBusClient.PublishAsync(message,
                                                 BusClientConstants.GlobalIssDictionariesExchangeName,
                                                 BusClientConstants.DataGlobalIssDictionariesRefreshKey,
                                                 _cancellationToken);
        });
    }

    #endregion Private Methods
}