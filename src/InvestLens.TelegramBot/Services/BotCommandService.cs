using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Extensions;

namespace InvestLens.TelegramBot.Services;

public class BotCommandService : IBotCommandService
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ISecuritiesRefreshStatusService _statusService;

    public BotCommandService(ITelegramNotificationService telegramNotificationService, ISecuritiesRefreshStatusService statusService)
    {
        _telegramNotificationService = telegramNotificationService;
        _statusService = statusService;
    }

    public async Task HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case "/health":
                await HealthOperation(cancellationToken);
                break;
            case "/info":
                await InfoOperation(cancellationToken);
                break;
            default:
                break;
        }
    }

    #region Private Methods

    private async Task InfoOperation(CancellationToken cancellationToken)
    {
        // получить из Redis статус загрузки Securities
        var refreshStatus = await _statusService.TryGetProgress();

        switch (refreshStatus.Status)
        {
            case SecuritiesRefreshStatus.None:
                await _telegramNotificationService.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Scheduled:
                await _telegramNotificationService.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Downloading:
                await _telegramNotificationService.NotifyStatusAsync("Обновление данных", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.DownloadedCount}", cancellationToken);
                break;
            case SecuritiesRefreshStatus.Processing:
                await _telegramNotificationService.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Saving:
                await _telegramNotificationService.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Completed:
                await _telegramNotificationService.NotifyOperationCompleteAsync(
                    refreshStatus.OperationId,
                    $"{refreshStatus.Status.GetDescription()}: {refreshStatus.DownloadedCount}/{refreshStatus.SavedCount}",
                    refreshStatus.Duration,
                    cancellationToken);
                break;
            case SecuritiesRefreshStatus.Failed:
                await _telegramNotificationService.NotifyErrorAsync(refreshStatus.OperationId, refreshStatus.ErrorMessage, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HealthOperation(CancellationToken cancellationToken)
    {
        await _telegramNotificationService.NotifyInfoAsync("Health Check", "Healthy", cancellationToken);
    }

    #endregion Private Methods
}