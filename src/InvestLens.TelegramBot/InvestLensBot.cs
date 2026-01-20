using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Extensions;
using InvestLens.TelegramBot.Data;

namespace InvestLens.TelegramBot;

public class InvestLensBot : BackgroundService
{
    private const string NextUpdateIdKey = "NextUpdateId";

    private readonly ISecuritiesRefreshStatusService _statusService;
    private readonly ITelegramService _telegramService;
    private readonly IRedisClient _redisClientForInstance;
    private readonly ILogger<InvestLensBot> _logger;

    public InvestLensBot(
        IRedisSettings redisSettings,
        ISecuritiesRefreshStatusService statusService,
        ITelegramService telegramService,
        IServiceProvider serviceProvider,
        ILogger<InvestLensBot> logger)
    {
        _redisClientForInstance = serviceProvider.GetKeyedService<IRedisClient>(redisSettings.InstanceName)!;
        _statusService = statusService;
        _telegramService = telegramService;
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
                await _telegramService.NotifyErrorAsync("Ошибка при проверке сообщений в чат-боте.", ex.Message, stoppingToken);
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
        var response = await _telegramService.GetUpdatesAsync(updateData.NextUpdateId, cancellationToken);

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
                switch (result.Message.Text)
                {
                    case "/info":
                        await InfoOperation(cancellationToken);
                        break;
                }

                updateData.NextUpdateId = result.UpdateId + 1;

                // 4. Обновить nextUpdateId в Redis
                await _redisClientForInstance.SetAsync(NextUpdateIdKey, updateData);
            }
        }
    }

    private async Task InfoOperation(CancellationToken cancellationToken)
    {
        // получить из Redis статус загрузки Securities
        var refreshStatus = await _statusService.TryGetProgress();

        switch (refreshStatus.Status)
        {
            case SecuritiesRefreshStatus.None:
                await _telegramService.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Scheduled:
                await _telegramService.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Downloading:
                await _telegramService.NotifyStatusAsync("Обновление данных", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.DownloadedCount}", cancellationToken);
                break;
            case SecuritiesRefreshStatus.Processing:
                await _telegramService.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Saving:
                await _telegramService.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Completed:
                await _telegramService.NotifyOperationCompleteAsync(
                    refreshStatus.OperationId, 
                    $"{refreshStatus.Status.GetDescription()}: {refreshStatus.DownloadedCount}/{refreshStatus.SavedCount}",
                    refreshStatus.Duration,
                    cancellationToken);
                break;
            case SecuritiesRefreshStatus.Failed:
                await _telegramService.NotifyErrorAsync(refreshStatus.OperationId, refreshStatus.ErrorMessage, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}