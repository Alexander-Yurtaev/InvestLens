using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Extensions;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Interfaces.Telegram.Services;
using InvestLens.Shared.Messages;
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
                await _telegramBotClient.NotifyStatusAsync($"Updating data: {refreshStatus.Duration:dd\\.hh\\:mm\\:ss}", $"{refreshStatus.Status.GetDisplayName()}: {refreshStatus.SavedCount:N0}/{refreshStatus.DownloadedCount:N0}", cancellationToken);
                break;
            case RefreshStatus.Completed:
                var message = new CompleteMessage
                {
                    CreatedAt = refreshStatus.StartedAt,
                    FinishedAt = refreshStatus.UpdatedAt
                };
                message.Headers.Add(HeaderConstants.CorrelationHeader, refreshStatus.CorrelationId);
                await _telegramBotClient.NotifyOperationCompleteAsync(
                    message,
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