using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Messages;
using Serilog.Context;

namespace InvestLens.Data.Api.Handlers;

public class GlobalIssDictionariesRefreshEventHandler : IMessageHandler<GlobalIssDictionariesRefreshMessage>
{
    private readonly IEngineDataPipeline _engineDataPipeline;
    private readonly IMarketDataPipeline _marketDataPipeline;
    private readonly IBoardDataPipeline _boardDataPipeline;
    private readonly IBoardGroupDataPipeline _boardGroupDataPipeline;
    private readonly IDurationDataPipeline _durationDataPipeline;
    private readonly ISecurityTypeDataPipeline _securityTypeDataPipeline;
    private readonly ISecurityGroupDataPipeline _securityGroupDataPipeline;
    private readonly ISecurityCollectionDataPipeline _securityCollectionDataPipeline;

    private readonly IPollyService _pollyService;
    private readonly IRefreshStatusService _statusService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IMessageBusClient _messageBus;
    private readonly ILogger<GlobalIssDictionariesRefreshEventHandler> _logger;

    public GlobalIssDictionariesRefreshEventHandler(
        IEngineDataPipeline engineDataPipeline,
        IMarketDataPipeline marketDataPipeline,
        IBoardDataPipeline boardDataPipeline,
        IBoardGroupDataPipeline boardGroupDataPipeline,
        IDurationDataPipeline durationDataPipeline,
        ISecurityTypeDataPipeline securityTypeDataPipeline,
        ISecurityGroupDataPipeline securityGroupDataPipeline,
        ISecurityCollectionDataPipeline securityCollectionDataPipeline,

        IPollyService pollyService,
        IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService,
        IMessageBusClient messageBus,
        ILogger<GlobalIssDictionariesRefreshEventHandler> logger)
    {
        _engineDataPipeline = engineDataPipeline;
        _marketDataPipeline = marketDataPipeline;
        _boardDataPipeline = boardDataPipeline;
        _boardGroupDataPipeline = boardGroupDataPipeline;
        _durationDataPipeline = durationDataPipeline;
        _securityTypeDataPipeline = securityTypeDataPipeline;
        _securityGroupDataPipeline = securityGroupDataPipeline;
        _securityCollectionDataPipeline = securityCollectionDataPipeline;

        _pollyService = pollyService;
        _statusService = statusService;
        _correlationIdService = correlationIdService;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(GlobalIssDictionariesRefreshMessage message, CancellationToken cancellationToken = default)
    {
        var correlationHeader = message.Headers
            .FirstOrDefault(h => h.Key.Equals(HeaderConstants.CorrelationHeader, StringComparison.OrdinalIgnoreCase));

        var correlationId = correlationHeader.Value?.ToString();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = _correlationIdService.GetOrCreateCorrelationId("SecurityRefreshEventHandler");
            _logger.LogWarning(
                "RabbitMQ message Id={MessageId} received without CorrelationId. Creating new: {CorrelationId}",
                message.MessageId, correlationId);
        }
        else
        {
            _correlationIdService.SetCorrelationId(correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation(
                "Received request to update Global ISS Dictionaries: {MessageId} от {MessageCreatedAt}.",
                message.MessageId, message.CreatedAt);

            var startedAt = await _statusService.Init(correlationId);

            try
            {
                await SendStartMessage("Global_Iss_Dictionaries", 
                    "Started loading: Global ISS Dictionaries.", startedAt, cancellationToken);

                var totalRecords = 0;

                totalRecords += await ProcessAllDataAsync(_engineDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_marketDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_boardDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_boardGroupDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_durationDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_securityTypeDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_securityGroupDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);
                totalRecords += await ProcessAllDataAsync(_securityCollectionDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);

                _logger.LogInformation(
                    "Completed updating: Global ISS Dictionaries: {MessageId} from {MessageCreatedAt}.",
                    message.MessageId, message.CreatedAt);

                await SendCompleteMessage("Global_Iss_Dictionaries", startedAt, totalRecords, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while updating Global ISS Dictionaries: {MessageId} from {MessageCreatedAt}.",
                    message.MessageId, message.CreatedAt);

                await _statusService.SetFailed(correlationId, ex.Message);
                await SendErrorMessage(correlationId, startedAt, ex, cancellationToken);

                return false;
            }
        }
    }

    #region Private Methods

    private async Task SendStartMessage(string correlationId, string details, DateTime startedAt, CancellationToken cancellationToken)
    {
        var message = new StartMessage()
        {
            CreatedAt = startedAt,
            Details = details
        };
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);

        var resilientPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramStartKey, cancellationToken);
        });
    }

    private async Task SendCompleteMessage(string correlationId, DateTime startedAt, int count, CancellationToken cancellationToken)
    {
        var message = new CompleteMessage()
        {
            CreatedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            Count = count
        };
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);

        var rabbitMqRetryPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await rabbitMqRetryPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramCompleteKey, cancellationToken);
        });
    }

    private async Task SendErrorMessage(string correlationId, DateTime startedAt, Exception exception,
        CancellationToken cancellationToken)
    {
        var message = new ErrorMessage(DateTime.UtcNow, exception.Message) { CreatedAt = startedAt };
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);

        var rabbitMqRetryPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await rabbitMqRetryPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramErrorKey, cancellationToken);
        });
    }

    private async Task<int> ProcessAllDataAsync(IDataPipeline dataPipeline, string correlationId, DateTime startedAt, Guid messageId, DateTime createdAt, CancellationToken cancellationToken)
    {
        await SendStartMessage(correlationId, $"Started loading: {dataPipeline.Info}.", startedAt, cancellationToken);
        var totalRecords = await dataPipeline.ProcessAllDataAsync(async (ex) =>
        {
            await SendErrorMessage(correlationId, startedAt, ex, cancellationToken);
        });

        _logger.LogInformation("Loading completed: {Info} {MessageId} от {MessageCreatedAt}.", dataPipeline.Info, messageId, createdAt);

        await _statusService.SetCompleted(correlationId, totalRecords);
        await SendCompleteMessage(correlationId, startedAt, totalRecords, cancellationToken);

        return totalRecords;
    }

    # endregion Private Methods
}
