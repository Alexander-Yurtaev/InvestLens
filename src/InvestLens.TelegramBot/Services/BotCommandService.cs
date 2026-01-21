using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Extensions;

namespace InvestLens.TelegramBot.Services;

public class BotCommandService : IBotCommandService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ISecuritiesRefreshStatusService _statusService;

    public BotCommandService(ITelegramBotClient telegramBotClient, ISecuritiesRefreshStatusService statusService)
    {
        _telegramBotClient = telegramBotClient;
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
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Scheduled:
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Downloading:
                await _telegramBotClient.NotifyStatusAsync("Обновление данных", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.DownloadedCount}", cancellationToken);
                break;
            case SecuritiesRefreshStatus.Processing:
                await _telegramBotClient.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Saving:
                await _telegramBotClient.NotifyStatusAsync("Обновление данных", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Completed:
                await _telegramBotClient.NotifyOperationCompleteAsync(
                    refreshStatus.OperationId,
                    $"{refreshStatus.Status.GetDescription()}: {refreshStatus.DownloadedCount}/{refreshStatus.SavedCount}",
                    refreshStatus.Duration,
                    cancellationToken);
                break;
            case SecuritiesRefreshStatus.Failed:
                await _telegramBotClient.NotifyErrorAsync(refreshStatus.OperationId, refreshStatus.ErrorMessage, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HealthOperation(CancellationToken cancellationToken)
    {
        await _telegramBotClient.NotifyInfoAsync("Health Check", "Healthy", cancellationToken);
    }

    #endregion Private Methods
}