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
    private readonly ISecuritiesRefreshStatusService _statusService;

    public BotCommandService(ITelegramBotClient telegramBotClient, ICorrelationIdService correlationIdService, ISecuritiesRefreshStatusService statusService)
    {
        _telegramBotClient = telegramBotClient;
        _correlationIdService = correlationIdService;
        _statusService = statusService;
    }

    public async Task HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdService.GetOrCreateCorrelationId("BotCommandService");
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            switch (command)
            {
                case "/health":
                    await HealthOperation(correlationId, cancellationToken);
                    break;
                case "/info":
                    await InfoOperation(correlationId, cancellationToken);
                    break;
            }
        }
    }

    #region Private Methods

    private async Task InfoOperation(string correlationId, CancellationToken cancellationToken)
    {
        // получить из Redis статус загрузки Securities
        var refreshStatus = await _statusService.TryGetProgress(correlationId);
        
        switch (refreshStatus.Status)
        {
            case SecuritiesRefreshStatus.None:
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.CorrelationId, refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Scheduled:
                await _telegramBotClient.NotifyInfoAsync(refreshStatus.CorrelationId, refreshStatus.Status.GetDisplayName(), refreshStatus.Status.GetDescription(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Downloading:
                await _telegramBotClient.NotifyStatusAsync(refreshStatus.CorrelationId, $"Обновление данных: {refreshStatus.Duration:dd\\.hh\\:mm\\:ss}", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.DownloadedCount:N0}", cancellationToken);
                break;
            case SecuritiesRefreshStatus.Processing:
                await _telegramBotClient.NotifyStatusAsync(refreshStatus.CorrelationId, $"Обновление данных: {refreshStatus.Duration:dd\\.hh\\:mm\\:ss}", refreshStatus.Status.GetDisplayName(), cancellationToken);
                break;
            case SecuritiesRefreshStatus.Saving:
                await _telegramBotClient.NotifyStatusAsync(refreshStatus.CorrelationId, $"Обновление данных: {refreshStatus.Duration:dd\\.hh\\:mm\\:ss}", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.SavedCount:N0}", cancellationToken);
                break;
            case SecuritiesRefreshStatus.Completed:
                await _telegramBotClient.NotifyOperationCompleteAsync(
                    refreshStatus.CorrelationId,
                    $"{refreshStatus.Status.GetDescription()}: {refreshStatus.DownloadedCount}/{refreshStatus.SavedCount}",
                    refreshStatus.Duration,
                    cancellationToken);
                break;
            case SecuritiesRefreshStatus.Failed:
                await _telegramBotClient.NotifyErrorAsync(refreshStatus.CorrelationId, refreshStatus.ErrorMessage, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HealthOperation(string correlationId, CancellationToken cancellationToken)
    {
        await _telegramBotClient.NotifyInfoAsync(correlationId, "Health Check", "Healthy", cancellationToken);
    }

    #endregion Private Methods
}