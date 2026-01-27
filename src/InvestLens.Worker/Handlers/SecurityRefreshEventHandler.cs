using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Models;
using InvestLens.Worker.Models.Settings;

namespace InvestLens.Worker.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private readonly IRedisClient _redisClient;
    private readonly IMessageBusClient _messageBus;
    private readonly IJobSettings _jobSettings;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    private SecuritiesRefreshStatus[] _idleStatuses =
        [SecuritiesRefreshStatus.None, SecuritiesRefreshStatus.Completed, SecuritiesRefreshStatus.Failed];

    public SecurityRefreshEventHandler(
        IRedisClient redisClient,
        IMessageBusClient messageBus,
        IJobSettings jobSettings,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _redisClient = redisClient;
        _messageBus = messageBus;
        _jobSettings = jobSettings;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг.");

        // 1. Проверяем, что в данный момент нет запущенной задачи для обновления данных
        var securitiesRefreshStatus = await _redisClient.GetAsync<SecuritiesRefreshProgress?>(RedisKeys.SecuritiesRefreshStatusRedisKey);
        if (securitiesRefreshStatus is not null) // есть запись о статусе
        {
            if (!_idleStatuses.Contains(securitiesRefreshStatus.Status)) // задача запущена
            {
                _logger.LogInformation("Обновление списка ценных бумаг уже запущено.");
                return true;
            }

            if (DateTimeHelper.IsRefreshed(securitiesRefreshStatus.UpdatedAt, _jobSettings.DelayBetweenRefresh)) // задача недавно завершилась
            {
                _logger.LogInformation("Списк ценных бумаг был обновлен недавно. Повторный запуск отменяется.");
                return true;
            }
        }

        // 2. Отправить сообщение сервису InvestLens.Data о том, что необходимо обновить данные
        var refreshingMessage = new SecurityRefreshingMessage();
        await _messageBus.PublishAsync(refreshingMessage, BusClientConstants.SecuritiesExchangeName, BusClientConstants.DataSecuritiesRefreshKey, cancellationToken);
        return true;
    }
}