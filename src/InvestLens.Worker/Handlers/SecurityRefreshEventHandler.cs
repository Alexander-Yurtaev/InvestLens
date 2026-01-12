using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Data;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Enums;
using InvestLens.Shared.Redis.Models;

namespace InvestLens.Worker.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private const string SecuritiesRefreshStatusRedisKey = "Securities.RefreshStatus";

    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisSettings _redisSettings;
    private readonly IMessageBusClient _messageBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    public SecurityRefreshEventHandler(
        IServiceProvider serviceProvider,
        IRedisSettings redisSettings,
        IMessageBusClient messageBus,
        IConfiguration configuration,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _redisSettings = redisSettings;
        _messageBus = messageBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг.");

        var redisClient = _serviceProvider.GetKeyedService<IRedisClient>(_redisSettings.InstanceName);
        if (redisClient is null)
        {
            _logger.LogError("{IRedisClient} for {InstanceName} is not registered.", nameof(IRedisClient), _redisSettings.InstanceName);
            throw new InvalidOperationException($"{nameof(IRedisClient)} for {_redisSettings.InstanceName} is not registered.");
        }

        // 1. Проверяем, что в данный момент нет запущенной задачи для обновления данных
        var securitiesRefreshStatus = await redisClient.GetAsync<SecuritiesRefreshState?>(SecuritiesRefreshStatusRedisKey);
        var runningStatuses = new[] { SecuritiesRefreshStatus.Refresh, SecuritiesRefreshStatus.Refreshing };
        if (securitiesRefreshStatus is not null) // есть запись о статусе
        {
            if (runningStatuses.Contains(securitiesRefreshStatus.Status)) // задача запущена
            {
                _logger.LogInformation("Таск для обновления списка ценных бумаг уже запущен.");
                return true;
            }

            if (securitiesRefreshStatus.FinishedAt is not null) // указана дата завершения
            {
                var delayBetweenRefresh = int.Parse(_configuration["DelayBetweenRefresh"] ?? "0");
                if (DateTimeHelper.IsExpired(securitiesRefreshStatus.FinishedAt.Value, delayBetweenRefresh)) // задача недавно завершилась
                {
                    _logger.LogInformation("Таск для обновления списка ценных бумаг завернился недавно. Повторный запуск отменяется.");
                    return true;
                }
            }
        }

        // 2. Отправить сообщение сервису InvestLens.Data о том, что необходимо обновить данные
        var refreshingMessage = new SecurityRefreshingMessage();

        await _messageBus.PublishAsync(new SecurityRefreshingMessage(), BusClientConstants.ExchangeName, BusClientConstants.SecuritiesRefreshingKey, cancellationToken);
        return true;
    }
}