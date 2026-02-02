using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.Worker.Services;

public class SecuritiesService : ISecuritiesService
{
    private readonly IMessageBusClient _messageBusClient;
    private readonly IPollyService _pollyService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<SecuritiesService> _logger;
    private readonly CancellationToken _cancellationToken;

    public SecuritiesService(IMessageBusClient messageBusClient, IPollyService pollyService,
        ICorrelationIdService correlationIdService, ILogger<SecuritiesService> logger)
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
                "Запуск ежедневного обновления списка ценных бумаг. CorrelationId: {CorrelationId}", correlationId));
        try
        {
            await RefreshSecuritiesAsync(correlationId);
            CorrelationHelper.CallLogWithCorrelationId(correlationId,
                () => _logger.LogInformation(
                    "Ежедневное обновление списка ценных бумаг завершено успешно. CorrelationId: {CorrelationId}",
                    correlationId));
        }
        catch (Exception ex)
        {
            CorrelationHelper.CallLogWithCorrelationId(correlationId,
                () => _logger.LogError(ex,
                    "Ежедневное обновление списка ценных бумаг завершилось с ошибкой. CorrelationId: {CorrelationId}",
                    correlationId));
            throw;
        }
    }

    public async Task InitializeApplicationAsync()
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId("init");
        await CorrelationHelper.CallLogWithCorrelationIdAsync(correlationId, async () =>
        {
            _logger.LogInformation("Запуск обновления списка ценных бумаг при инициализации приложения.");
            try
            {
                await RefreshSecuritiesAsync(correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Запуск обновление списка ценных бумаг при инициализации приложения завершилось с ошибкой.");
                throw;
            }
        });
    }

    #region Private Methods

    private async Task RefreshSecuritiesAsync(string correlationId)
    {
        var resilientPolicy = _pollyService.GetRabbitMqResilientPolicy();
        var message = new SecurityRefreshMessage();
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBusClient.PublishAsync(message,
                                                 BusClientConstants.SecuritiesExchangeName,
                                                 BusClientConstants.DataSecuritiesRefreshKey,
                                                 _cancellationToken);
        });
    }

    #endregion Private Methods
}