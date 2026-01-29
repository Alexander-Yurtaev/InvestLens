using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Models;
using Serilog.Context;

namespace InvestLens.Data.Api.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private readonly IMoexClient _moexClient;
    private readonly IPollyService _pollyService;
    private readonly ISecuritiesRefreshStatusService _statusService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IMessageBusClient _messageBus;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    public SecurityRefreshEventHandler(
        IMoexClient moexClient,
        IPollyService pollyService,
        ISecuritiesRefreshStatusService statusService,
        ICorrelationIdService correlationIdService,
        IMessageBusClient messageBus,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _moexClient = moexClient;
        _pollyService = pollyService;
        _statusService = statusService;
        _correlationIdService = correlationIdService;
        _messageBus = messageBus;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        var correlationHeader = message.Headers
            .FirstOrDefault(h => h.Key.Equals(HeaderConstants.CorrelationHeader, StringComparison.OrdinalIgnoreCase));

        var correlationId = correlationHeader.Value?.ToString();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = _correlationIdService.GetOrCreateCorrelationId("rabbitmq");
            _logger.LogWarning(
                "RabbitMQ-сообщение Id={MessageId} пришло без CorrelationId. Создаем новое: {CorrelationId}.",
                message.MessageId, correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation(
                "Получено поручение обновить список ценных бумаг: {MessageId} от {MessageCreatedAt}. CorrelationId: {CorrelationId}",
                message.MessageId, message.CreatedAt, correlationId);

            try
            {
                var startedAt = await _statusService.Init(correlationId);
                await SendStartMessage(correlationId, startedAt, cancellationToken);
                await RefreshSecurities(correlationId, startedAt, cancellationToken);
                _logger.LogInformation(
                    "Cписок ценных бумаг обновлен: {MessageId} от {MessageCreatedAt}. CorrelationId: {CorrelationId}",
                    message.MessageId, message.CreatedAt, correlationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при обновлении списка ценных бумаг: {MessageId} от {MessageCreatedAt}. CorrelationId: {CorrelationId}",
                    message.MessageId, message.CreatedAt, correlationId);
                return false;
            }
        }
    }

    #region Private Methods

    private async Task RefreshSecurities(string correlationId, DateTime startedAt, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Получаем данные от MOEX
            var securitiesResponse = await _moexClient.GetSecurities(correlationId);
            if (securitiesResponse is null || securitiesResponse.Securities.Data.Length == 0)
            {
                throw new InvalidOperationException("Не были получены данные от MOEX.");
            }

            // 2. Конвертируем данные из Response в Entity
            await _statusService.SetProcessing(correlationId);
            var securities = ResponseToEntityConverters.SecurityResponseToEntityConverter(securitiesResponse);

            // 3. Сохраняем/обновляем данные в БД
            await _statusService.SetSaving(correlationId);
            var affected = await _securityRepository.Add(securities, true);

            await _statusService.SetCompleted(correlationId, affected);

            // 4. Обновляем RefreshStatus
            await _refreshStatusRepository.SetRefreshStatus(DatabaseConstants.SecurityEntityName);

            await SendCompleteMessage(correlationId, startedAt, securities.Count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных.");

            await _statusService.SetFailed(correlationId, ex.Message);
            await SendErrorMessage(correlationId, startedAt, ex, cancellationToken);

            throw;
        }
    }

    private async Task SendStartMessage(string correlationId, DateTime startedAt, CancellationToken cancellationToken)
    {
        var message = new StartMessage()
        {
            CreatedAt = startedAt,
            Details = "Началась загрузка списка ценных бумаг на MOEX."
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
        var message = new ErrorMessage(DateTime.UtcNow, exception) { CreatedAt = startedAt };
        message.Headers.Add(HeaderConstants.CorrelationHeader, correlationId);
        
        var rabbitMqRetryPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await rabbitMqRetryPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramErrorKey, cancellationToken);
        });
    }

    # endregion Private Methods
}