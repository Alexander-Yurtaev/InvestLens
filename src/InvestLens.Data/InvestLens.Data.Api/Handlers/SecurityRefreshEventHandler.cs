using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Messages;
using Serilog.Context;

namespace InvestLens.Data.Api.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private readonly ISecurityDataPipeline _securityDataPipeline;

    private readonly IPollyService _pollyService;
    private readonly IRefreshStatusService _statusService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IMessageBusClient _messageBus;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    public SecurityRefreshEventHandler(
        ISecurityDataPipeline securityDataPipeline,
        IPollyService pollyService,
        IRefreshStatusService statusService,
        ICorrelationIdService correlationIdService,
        IMessageBusClient messageBus,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _securityDataPipeline = securityDataPipeline;
        
        _pollyService = pollyService;
        _statusService = statusService;
        _correlationIdService = correlationIdService;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
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
                "Received request to update securities list: {MessageId} от {MessageCreatedAt}.",
                message.MessageId, message.CreatedAt);

            var startedAt = await _statusService.Init(correlationId);

            try
            {
                await ProcessAllDataAsync(_securityDataPipeline, correlationId, startedAt, message.MessageId, message.CreatedAt, cancellationToken);

                _logger.LogInformation(
                    "Completed updating: Securities: {MessageId} from {MessageCreatedAt}.",
                    message.MessageId, message.CreatedAt);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while updating securities list: {MessageId} from {MessageCreatedAt}.",
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

    private async Task ProcessAllDataAsync(IDataPipeline dataPipeline, string correlationId, DateTime startedAt, Guid messageId, DateTime createdAt, CancellationToken cancellationToken)
    {
        await SendStartMessage(correlationId, $"Началась загрузка: {dataPipeline.Info}.", startedAt, cancellationToken);
        var totalRecords = await dataPipeline.ProcessAllDataAsync(async (ex) =>
        {
            await SendErrorMessage(correlationId, startedAt, ex, cancellationToken);
        });

        _logger.LogInformation("Загрузка завершена: {Info} {MessageId} от {MessageCreatedAt}.", dataPipeline.Info, messageId, createdAt);

        await _statusService.SetCompleted(correlationId, totalRecords);
        await SendCompleteMessage(correlationId, startedAt, totalRecords, cancellationToken);
    }

    # endregion Private Methods
}