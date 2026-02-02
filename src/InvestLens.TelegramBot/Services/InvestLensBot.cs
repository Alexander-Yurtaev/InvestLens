using InvestLens.Abstraction.Models.Telegram;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.TelegramBot.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Context;

namespace InvestLens.TelegramBot.Services;

public class InvestLensBot : BackgroundService, IHealthCheck
{
    private const string NextUpdateIdKey = "NextUpdateId";

    private volatile bool _isHealthy = true;
    private readonly ITelegramBotClient _botClient;
    private readonly IBotCommandService _botCommandService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IRedisClient _redisClientForInstance;
    private readonly ILogger<InvestLensBot> _logger;

    private DateTime _lastProcessedTime = DateTime.UtcNow;

    public InvestLensBot(
        IRedisSettings redisSettings,
        ITelegramBotClient botClient,
        IBotCommandService botCommandService,
        ICorrelationIdService correlationIdService,
        IServiceProvider serviceProvider,
        ILogger<InvestLensBot> logger)
    {
        _redisClientForInstance = serviceProvider.GetKeyedService<IRedisClient>(redisSettings.InstanceName)!;
        _botClient = botClient;
        _botCommandService = botCommandService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId("TelegramBot");
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Telegram Bot Service starting...");

            await InitAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updateData = await _redisClientForInstance.GetAsync<UpdateData>(NextUpdateIdKey) ?? new UpdateData();

                    var updates = await GetUpdatesOperation(updateData.NextUpdateId, stoppingToken);
                    if (updates is not null)
                    {
                        foreach (var result in updates.Result)
                        {
                            await _botCommandService.HandleCommandAsync(result.Message.Text, stoppingToken);

                            updateData.NextUpdateId = result.UpdateId + 1;
                            _lastProcessedTime = DateTime.UtcNow;

                            // 4. Обновить nextUpdateId в Redis
                            await _redisClientForInstance.SetAsync(NextUpdateIdKey, updateData);
                        }
                        _isHealthy = true;
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при проверке сообщений в чат-боте.");
                    await _botClient.NotifyErrorAsync("Ошибка при проверке сообщений в чат-боте: {ex.Message}", stoppingToken);
                }
            }

            _logger.LogInformation("Telegram Bot Service stopping...");
        }
    }

    private async Task InitAsync(CancellationToken cancellationToken)
    {
        var me = await _botClient.GetMeAsync(cancellationToken);
        if (me?.Result is null)
        {
            _logger.LogWarning($"Bot return NULL on getMe request.");
        }
        else
        {
            _logger.LogInformation($"Bot @{me.Result.Username} started");
        }

        // NextUpdateId
        var isExists = await _redisClientForInstance.ExistsAsync(NextUpdateIdKey, cancellationToken);
        if (!isExists)
        {
            await _redisClientForInstance.SetAsync(NextUpdateIdKey, new UpdateData());
        }
    }

    private async Task<GetUpdatesResponse?> GetUpdatesOperation(int nextUpdateId, CancellationToken cancellationToken)
    {
        // 1. Получить из Telegram данные
        var response = await _botClient.GetUpdatesAsync(nextUpdateId, cancellationToken);

        // 2. Обработать полученные данные
        if (response is null)
        {
            _logger.LogWarning("От Telegram не пришли данные.");
            return null;
        }

        if (!response.Ok)
        {
            _logger.LogWarning("От Telegram получена ошибка: {ErrorCode} - {Description}", response.ErrorCode, response.Description);
            return null;
        }

        return response;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!_isHealthy)
            return Task.FromResult(HealthCheckResult.Unhealthy("Bot is in error state"));

        // Проверяем, когда последний раз обрабатывали сообщение
        if (DateTime.UtcNow - _lastProcessedTime > TimeSpan.FromMinutes(5))
            return Task.FromResult(
                HealthCheckResult.Degraded("No messages processed in last 5 minutes"));

        return Task.FromResult(
            HealthCheckResult.Healthy($"Last processed: {_lastProcessedTime:HH:mm:ss}"));
    }
}