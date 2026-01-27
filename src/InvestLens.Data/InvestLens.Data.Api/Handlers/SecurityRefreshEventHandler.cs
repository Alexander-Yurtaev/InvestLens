using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.Data.Api.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private readonly IMoexClient _moexClient;
    private readonly IPollyService _pollyService;
    private readonly ISecuritiesRefreshStatusService _statusService;
    private readonly IMessageBusClient _messageBus;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    public SecurityRefreshEventHandler(
        IMoexClient moexClient,
        IPollyService pollyService,
        ISecuritiesRefreshStatusService statusService,
        IMessageBusClient messageBus,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _moexClient = moexClient;
        _pollyService = pollyService;
        _statusService = statusService;
        _messageBus = messageBus;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);

        try
        {
            var progress = await _statusService.Init();
            await SendStartMessage(progress.Item1, progress.Item2, cancellationToken);
            await RefreshSecurities(progress.Item1, progress.Item2, cancellationToken);
            _logger.LogInformation("Cписок ценных бумаг обновлен: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении списка ценных бумаг: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);
            return false;
        }
    }

    #region Private Methods

    private async Task RefreshSecurities(Guid correlationId, DateTime startedAt, CancellationToken cancellationToken)
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
            await _statusService.SetProcessing();
            var securities = ResponseToEntityConverters.SecurityResponseToEntityConverter(securitiesResponse);

            // 3. Сохраняем/обновляем данные в БД
            await _statusService.SetSaving();
            var affected = await _securityRepository.Add(securities, true);

            await _statusService.SetCompleted(affected);

            // 4. Обновляем RefreshStatus
            await _refreshStatusRepository.SetRefreshStatus(DatabaseConstants.SecurityEntityName);

            await SendCompleteMessage(correlationId, startedAt, securities.Count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных.");

            await _statusService.SetFailed(ex.Message);
            await SendErrorMessage(correlationId, startedAt, ex, cancellationToken);

            throw;
        }
    }

    private async Task SendStartMessage(Guid correlationId, DateTime startedAt, CancellationToken cancellationToken)
    {
        var message = new StartMessage(correlationId)
        {
            CreatedAt = startedAt,
            Details = "Началась загрузка списка ценных бумаг на MOEX."
        };
        // ToDo исправить на актуальный Exception.
        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramStartKey, cancellationToken);
        });
    }

    private async Task SendCompleteMessage(Guid correlationId, DateTime startedAt, int count, CancellationToken cancellationToken)
    {
        var message = new CompleteMessage(correlationId)
        {
            CreatedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            Count = count
        };
        var rabbitMqRetryPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await rabbitMqRetryPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramCompleteKey, cancellationToken);
        });
    }

    private async Task SendErrorMessage(Guid correlationId, DateTime startedAt, Exception exception,
        CancellationToken cancellationToken)
    {
        var rabbitMqRetryPolicy = _pollyService.GetRabbitMqRetryPolicy();
        await rabbitMqRetryPolicy.ExecuteAndCaptureAsync(async () =>
        {
            var message = new ErrorMessage(correlationId, DateTime.UtcNow, exception) { CreatedAt = startedAt };
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramErrorKey, cancellationToken);
        });
    }

    # endregion Private Methods
}