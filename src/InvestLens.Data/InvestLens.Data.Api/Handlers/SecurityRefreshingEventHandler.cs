using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Models;

namespace InvestLens.Data.Api.Handlers;

public class SecurityRefreshingEventHandler : IMessageHandler<SecurityRefreshingMessage>
{
    private readonly IMoexClient _moexClient;
    private readonly IPollyService _pollyService;
    private readonly IRedisClient _redisClient;
    private readonly IMessageBusClient _messageBus;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly ILogger<SecurityRefreshingEventHandler> _logger;

    public SecurityRefreshingEventHandler(
        IMoexClient moexClient,
        IPollyService pollyService,
        IRedisClient redisClient,
        IMessageBusClient messageBus,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        ILogger<SecurityRefreshingEventHandler> logger)
    {
        _moexClient = moexClient;
        _pollyService = pollyService;
        _redisClient = redisClient;
        _messageBus = messageBus;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshingMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);

        try
        {
            await RefreshSecurities();
            _logger.LogInformation("Cписок ценных бумаг обновлен: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении списка ценных бумаг: {MessageId} от {MessageCreatedAt}.", message.MessageId, message.CreatedAt);
            return false;
        }
    }

    private async Task RefreshSecurities()
    {
        // ToDo исправить на актуальный Exception.
        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
        var securitiesRefreshStatus = await resilientPolicy.ExecuteAsync(async () =>
            await _redisClient.GetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey));
        if (securitiesRefreshStatus is null)
        {
            _logger.LogError("В Redis отсутствует информация о SecuritiesRefreshStatus.");
            return;
        }

        //
        securitiesRefreshStatus.Start();
        await _redisClient.SetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey,
            securitiesRefreshStatus, TimeSpan.FromHours(24));

        var operationId = Guid.NewGuid().ToString();
        DateTime startedAt = DateTime.UtcNow;

        try
        {
            await SendStartMessage(operationId, startedAt, CancellationToken.None);
            // 1. Получаем данные от MOEX
            var securitiesResponse = await _moexClient.GetSecurities();
            if (securitiesResponse is null || securitiesResponse.Securities.Data.Length == 0)
            {
                throw new InvalidOperationException("Не были получены данные от MOEX.");
            }

            // 2. Конвертируем данные из Response в Entity
            var securities = ResponseToEntityConverters.SecurityResponseToEntityConverter(securitiesResponse);

            // 3. Сохраняем/обновляем данные в БД
            await _securityRepository.Add(securities, true);

            // 4. Обновляем RefreshStatus
            await _refreshStatusRepository.SetRefreshStatus(DatabaseConstants.SecurityEntityName);

            //
            securitiesRefreshStatus.Finish();
            await _redisClient.SetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey, securitiesRefreshStatus, TimeSpan.FromHours(24));

            await SendCompleteMessage(operationId, startedAt, securities.Count, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных.");

            await SendErrorMessage(operationId, startedAt, ex, CancellationToken.None);

            securitiesRefreshStatus.Reset();
            await _redisClient.SetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey, securitiesRefreshStatus, TimeSpan.FromHours(24));

            throw;
        }
    }

    private async Task SendStartMessage(string operationId, DateTime startedAt, CancellationToken cancellationToken)
    {
        var message = new StartMessage(operationId)
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

    private async Task SendCompleteMessage(string operationId, DateTime startedAt, int count, CancellationToken cancellationToken)
    {
        var message = new CompleteMessage(operationId)
        {
            CreatedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            Count = count
        };
        // ToDo исправить на актуальный Exception.
        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramCompleteKey, cancellationToken);
        });
    }

    private async Task SendErrorMessage(string operationId, DateTime startedAt, Exception exception,
        CancellationToken cancellationToken)
    {
        // ToDo исправить на актуальный Exception.
        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            var message = new ErrorMessage(operationId, DateTime.UtcNow, exception) { CreatedAt = startedAt };
            await _messageBus.PublishAsync(message, BusClientConstants.TelegramExchangeName,
                BusClientConstants.TelegramErrorKey, cancellationToken);
        });
    }
}