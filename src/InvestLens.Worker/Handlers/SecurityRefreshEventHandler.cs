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

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private readonly IRedisClient _redisClient;
    private readonly IMessageBusClient _messageBus;
    private readonly IJobSettings _jobSettings;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    private readonly SecuritiesRefreshStatus[] _idleStatuses =
        [SecuritiesRefreshStatus.None, SecuritiesRefreshStatus.Completed, SecuritiesRefreshStatus.Failed];

    public SecurityRefreshEventHandler(
        IRedisClient redisClient,
        IMessageBusClient messageBus,
        IJobSettings jobSettings,
        ICorrelationIdService correlationIdService,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _redisClient = redisClient;
        _messageBus = messageBus;
        _jobSettings = jobSettings;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено задание на обновление списка ценных бумаг.");

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
            try
            {
                // 1. Проверяем, что в данный момент не выполняется задача обновления данных
                var securitiesRefreshStatus = await _redisClient.GetAsync<SecuritiesRefreshProgress?>(RedisKeys.SecuritiesRefreshStatusRedisKey);

                if (securitiesRefreshStatus is not null)
                {
                    // Проверяем, выполняется ли задача в данный момент
                    if (!_idleStatuses.Contains(securitiesRefreshStatus.Status))
                    {
                        _logger.LogInformation("Обновление списка ценных бумаг уже выполняется. Статус: {Status}",
                            securitiesRefreshStatus.Status);
                        return true;
                    }

                    // Проверяем, не было ли недавнего успешного обновления
                    if (DateTimeHelper.IsRefreshed(securitiesRefreshStatus.UpdatedAt, _jobSettings.DelayBetweenRefresh))
                    {
                        _logger.LogInformation(
                            "Список ценных бумаг был обновлен {UpdatedAt}. " +
                            "Следующее обновление возможно через {Delay} минут. Задача отменена.",
                            securitiesRefreshStatus.UpdatedAt,
                            _jobSettings.DelayBetweenRefresh.TotalMinutes);
                        return true;
                    }
                }

                // 2. Отправляем сообщение сервису InvestLens.Data для обновления данных
                // Используем тот же тип сообщения, что был получен
                await _messageBus.PublishAsync(
                    message, // Используем исходное сообщение
                    BusClientConstants.SecuritiesExchangeName,
                    BusClientConstants.DataSecuritiesRefreshKey,
                    cancellationToken);

                _logger.LogInformation("Задание на обновление списка ценных бумаг отправлено в очередь.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке задания обновления списка ценных бумаг.");
                return false; // Возвращаем false для NACK и отправки в DLQ
            }
        }
    }
}