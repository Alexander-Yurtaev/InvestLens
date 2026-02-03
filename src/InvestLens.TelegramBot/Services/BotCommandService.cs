using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Extensions;
using Serilog.Context;

namespace InvestLens.TelegramBot.Services;

public class BotCommandService : IBotCommandService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IRefreshStatusService _statusService;

    public BotCommandService(ITelegramBotClient telegramBotClient, ICorrelationIdService correlationIdService, IRefreshStatusService statusService)
    {
        _telegramBotClient = telegramBotClient;
        _correlationIdService = correlationIdService;
        _statusService = statusService;
    }

    public async Task HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId(nameof(BotCommandService));
        using (LogContext.PushProperty("CorrelationId", correlationId))
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
    }

    #region Private Methods

    private async Task InfoOperation(CancellationToken cancellationToken)
    {
        // получить из Redis статус загрузки Securities
        var refreshStatus = await _statusService.TryGetProgress(_correlationIdService.GetOrCreateCorrelationId(nameof(BotCommandService)));
        
        switch (refreshStatus.Status)
        {
            case RefreshStatus.None:
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case RefreshStatus.Scheduled:
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case RefreshStatus.Processing:
                await _telegramBotClient.NotifyStatusAsync($"Обновление данных: {refreshStatus.Duration:dd\\.hh\\:mm\\:ss}", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.SavedCount:N0}/{refreshStatus.DownloadedCount:N0}", cancellationToken);
                break;
            case RefreshStatus.Completed:
                await _telegramBotClient.NotifyOperationCompleteAsync(
                    $"{refreshStatus.Status.GetDescription()}: {refreshStatus.SavedCount}/{refreshStatus.DownloadedCount}",
                    refreshStatus.Duration,
                    cancellationToken);
                break;
            case RefreshStatus.Failed:
                await _telegramBotClient.NotifyErrorAsync(refreshStatus.ErrorMessage, cancellationToken);
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