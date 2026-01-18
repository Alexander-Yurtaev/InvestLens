namespace InvestLens.Abstraction.Services;

public interface ITelegramNotificationService
{
    Task NotifyAsync(string message, CancellationToken cancellationToken = default);
    Task NotifyErrorAsync(string operation, Exception exception, CancellationToken cancellationToken = default);
    Task NotifyOperationStartAsync(string operationId, string details, CancellationToken cancellationToken = default);
    Task NotifyOperationCompleteAsync(string operationId, string result, TimeSpan duration, CancellationToken cancellationToken = default);
}