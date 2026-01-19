namespace InvestLens.Shared.MessageBus.Models;

public class ErrorMessage : BaseTelegramMessage
{
    public ErrorMessage(string operationId, DateTime finishedAt, Exception? exception) : base(operationId)
    {
        FinishedAt = finishedAt;
        Exception = exception;
    }

    public Exception? Exception { get; set; }
}