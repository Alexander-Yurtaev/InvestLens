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
    private readonly IRedisClient _redisClient;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly ILogger<SecurityRefreshingEventHandler> _logger;

    public SecurityRefreshingEventHandler(
        IMoexClient moexClient,
        IRedisClient redisClient,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        ILogger<SecurityRefreshingEventHandler> logger)
    {
        _moexClient = moexClient;
        _redisClient = redisClient;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshingMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг.");

        try
        {
            await RefreshSecurities();
            _logger.LogInformation("Cписок ценных бумаг обновлен.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении списка ценных бумаг.");
            return false;
        }
    }

    private async Task RefreshSecurities()
    {
        var securitiesRefreshStatus = await _redisClient.GetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey);
        if (securitiesRefreshStatus is null)
        {
            _logger.LogError("В Redis отсутствует информация и SecuritiesRefreshStatus.");
            return;
        }

        //
        securitiesRefreshStatus.Start();
        await _redisClient.SetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey, securitiesRefreshStatus, TimeSpan.FromHours(24));

        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных.");

            securitiesRefreshStatus.Reset();
            await _redisClient.SetAsync<SecuritiesRefreshState?>(RedisKeys.SecuritiesRefreshStatusRedisKey, securitiesRefreshStatus, TimeSpan.FromHours(24));

            throw;
        }
    }
}