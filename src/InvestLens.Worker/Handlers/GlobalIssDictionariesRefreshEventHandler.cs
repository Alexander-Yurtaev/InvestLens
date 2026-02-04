using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Models;
using InvestLens.Worker.Models.Settings;
using Serilog.Context;

namespace InvestLens.Worker.Handlers;

public class GlobalIssDictionariesRefreshEventHandler : IMessageHandler<GlobalIssDictionariesRefreshMessage>
{
    private readonly IRedisClient _redisClient;
    private readonly IMessageBusClient _messageBus;
    private readonly IJobSettings _jobSettings;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<GlobalIssDictionariesRefreshEventHandler> _logger;

    private readonly RefreshStatus[] _idleStatuses =
        [RefreshStatus.None, RefreshStatus.Completed, RefreshStatus.Failed];

    public GlobalIssDictionariesRefreshEventHandler(IRedisClient redisClient,
        IMessageBusClient messageBus,
        IJobSettings jobSettings,
        ICorrelationIdService correlationIdService,
        ILogger<GlobalIssDictionariesRefreshEventHandler> logger)
    {
        _redisClient = redisClient;
        _messageBus = messageBus;
        _jobSettings = jobSettings;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(GlobalIssDictionariesRefreshMessage message, CancellationToken cancellationToken = default)
    {
        var correlationHeader = message.Headers
            .FirstOrDefault(h => h.Key.Equals(HeaderConstants.CorrelationHeader, StringComparison.OrdinalIgnoreCase));

        var correlationId = correlationHeader.Value?.ToString();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = _correlationIdService.GetOrCreateCorrelationId("GlobalIssDictionariesRefreshEventHandler");
            _logger.LogWarning(
                "RabbitMQ-сообщение Id={MessageId} пришло без CorrelationId. Создаем новое: {CorrelationId}.",
                message.MessageId, correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Получено задание на обновление списка ценных бумаг.");

            try
            {
                // 1. Проверяем, что в данный момент не выполняется задача обновления данных
                var globalIssDictionariesRefreshStatus = await _redisClient.GetAsync<RefreshProgress?>(RedisKeys.GlobalIssDictionariesRefreshStatusRedisKey);

                var busyStatuses = new List<RefreshStatus>() { RefreshStatus.Scheduled, RefreshStatus.Processing };

                if (globalIssDictionariesRefreshStatus is not null && busyStatuses.Contains(globalIssDictionariesRefreshStatus.Status))
                {
                    // Проверяем, выполняется ли задача в данный момент
                    if (!_idleStatuses.Contains(globalIssDictionariesRefreshStatus.Status))
                    {
                        _logger.LogInformation("Обновление глобавльных справочников ISS. Статус: {Status}",
                            globalIssDictionariesRefreshStatus.Status);
                        return true;
                    }

                    // Проверяем, не было ли недавнего успешного обновления
                    if (DateTimeHelper.IsRefreshed(globalIssDictionariesRefreshStatus.UpdatedAt, _jobSettings.DelayBetweenRefresh))
                    {
                        _logger.LogInformation(
                            "Глобальные справочники ISS обновлены {UpdatedAt}. " +
                            "Следующее обновление возможно через {Delay} минут. Задача отменена.",
                            globalIssDictionariesRefreshStatus.UpdatedAt,
                            _jobSettings.DelayBetweenRefresh.TotalMinutes);
                        return true;
                    }
                }

                // 2. Отправляем сообщение сервису InvestLens.Data для обновления данных
                // Используем тот же тип сообщения, что был получен
                await _messageBus.PublishAsync(
                    message, // Используем исходное сообщение
                    BusClientConstants.GlobalIssDictionariesExchangeName,
                    BusClientConstants.DataGlobalIssDictionariesRefreshKey,
                    cancellationToken);

                _logger.LogInformation("Задание на обновление глобальных справночник ISS отправлено в очередь.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке задания обновления глобальных справочников ISS.");
                return false; // Возвращаем false для NACK и отправки в DLQ
            }
        }
    }
}