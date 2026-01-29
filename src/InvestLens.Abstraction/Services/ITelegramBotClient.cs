using InvestLens.Abstraction.Models.Telegram;

namespace InvestLens.Abstraction.Services;

public interface ITelegramBotClient
{
    Task NotifyAsync(string message, CancellationToken cancellationToken = default);
    Task NotifyOperationStartAsync(string correlationId, string details, CancellationToken cancellationToken = default);
    Task NotifyOperationCompleteAsync(string correlationId, string result, TimeSpan duration, CancellationToken cancellationToken = default);
    Task NotifyInfoAsync(string correlationId, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyErrorAsync(string correlationId, string exceptionMessage, CancellationToken cancellationToken = default);
    Task NotifyWarningAsync(string correlationId, string warning, string details = "", CancellationToken cancellationToken = default);
    Task NotifyStatusAsync(string correlationId, string status, string currentState, CancellationToken cancellationToken = default);
    Task NotifyDataUpdateAsync(string correlationId, string dataType, int count, string description = "", CancellationToken cancellationToken = default);
    Task NotifyScheduledTaskAsync(string correlationId, string taskName, string result, CancellationToken cancellationToken = default);
    Task NotifyHeartbeatAsync(string correlationId, string serviceName, TimeSpan uptime, CancellationToken cancellationToken = default);
    Task NotifyCustomAsync(string emoji, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyPlainAsync(string message, CancellationToken cancellationToken = default);

    Task<GetUpdatesResponse?> GetUpdatesAsync(int nextUpdateId, CancellationToken cancellationToken = default);
    Task<GetMeResponse?> GetMeAsync(CancellationToken cancellationToken);
}