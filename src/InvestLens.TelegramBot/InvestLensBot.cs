using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.TelegramBot.Data;

namespace InvestLens.TelegramBot;

public class InvestLensBot : BackgroundService
{
    private const string NextUpdateIdKey = "NextUpdateId";

    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IBotCommandService _botCommandService;
    private readonly IRedisClient _redisClientForInstance;
    private readonly ILogger<InvestLensBot> _logger;

    public InvestLensBot(
        IRedisSettings redisSettings,
        ITelegramNotificationService telegramNotificationService,
        IBotCommandService botCommandService,
        IServiceProvider serviceProvider,
        ILogger<InvestLensBot> logger)
    {
        _redisClientForInstance = serviceProvider.GetKeyedService<IRedisClient>(redisSettings.InstanceName)!;
        _telegramNotificationService = telegramNotificationService;
        _botCommandService = botCommandService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GetUpdatesOperation(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке сообщений в чат-боте.");
                await _telegramNotificationService.NotifyErrorAsync("Ошибка при проверке сообщений в чат-боте.", ex.Message, stoppingToken);
            }
        }
    }

    private async Task InitAsync()
    {
        // NextUpdateId
        var isExists = await _redisClientForInstance.ExistsAsync(NextUpdateIdKey);
        if (!isExists)
        {
            await _redisClientForInstance.SetAsync(NextUpdateIdKey, new UpdateData());
        }
    }

    private async Task GetUpdatesOperation(CancellationToken cancellationToken)
    {
        // 1. Получить из Redis nextUpdateId
        var updateData = await _redisClientForInstance.GetAsync<UpdateData>(NextUpdateIdKey) ?? new UpdateData();

        // 2. Получить из Telegram данные
        var response = await _telegramNotificationService.GetUpdatesAsync(updateData.NextUpdateId, cancellationToken);

        // 3. Обработать полученные данные
        if (response is null)
        {
            _logger.LogWarning("От Telegram не пришли данные.");
        }
        else if (!response.Ok)
        {
            _logger.LogWarning("От Telegram получена ошибка: {ErrorCode} - {Description}", response.ErrorCode, response.Description);
        }
        else
        {
            foreach (var result in response.Result)
            {
                await _botCommandService.HandleCommandAsync(result.Message.Text, cancellationToken);

                updateData.NextUpdateId = result.UpdateId + 1;

                // 4. Обновить nextUpdateId в Redis
                await _redisClientForInstance.SetAsync(NextUpdateIdKey, updateData);
            }
        }
    }
}