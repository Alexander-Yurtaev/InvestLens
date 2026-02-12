using InvestLens.Abstraction.Telegram.Models;
using InvestLens.Shared.Interfaces.MessageBus.Models;
using InvestLens.Shared.Interfaces.Telegram.Models;

namespace InvestLens.Shared.Interfaces.Telegram.Services;

public interface ITelegramBotClient
{
    Task NotifyAsync(string message, CancellationToken cancellationToken = default);
    Task NotifyOperationStartAsync(IBaseMessage baseMessage, string details, CancellationToken cancellationToken = default);
    Task NotifyOperationCompleteAsync(IBaseMessage baseMessage, string result, TimeSpan duration, CancellationToken cancellationToken = default);
    Task NotifyInfoAsync(string title, string message, CancellationToken cancellationToken = default);
    Task NotifyErrorAsync(IBaseMessage baseMessage, string exceptionMessage, CancellationToken cancellationToken = default);
    Task NotifyWarningAsync(string warning, string details = "", CancellationToken cancellationToken = default);
    Task NotifyStatusAsync(string status, string currentState, CancellationToken cancellationToken = default);
    Task NotifyDataUpdateAsync(string dataType, int count, string description = "", CancellationToken cancellationToken = default);
    Task NotifyScheduledTaskAsync(string taskName, string result, CancellationToken cancellationToken = default);
    Task NotifyHeartbeatAsync(string serviceName, TimeSpan uptime, CancellationToken cancellationToken = default);
    Task NotifyCustomAsync(string emoji, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyPlainAsync(string message, CancellationToken cancellationToken = default);

    Task<GetUpdatesResponse?> GetUpdatesAsync(int nextUpdateId, CancellationToken cancellationToken = default);
    Task<GetMeResponse?> GetMeAsync(CancellationToken cancellationToken);
}