using InvestLens.Abstraction.DTOs;

namespace InvestLens.Shared.MessageBus.Models;

public class ErrorMessage : BaseTelegramMessage
{
    public ErrorMessage(Guid correlationId, DateTime finishedAt, Exception? exception) : base(correlationId)
    {
        FinishedAt = finishedAt;
        Exception = new ExceptionDto(exception?.Message ?? "Произошла ошибка", "SECURITY_REFRESH_ERROR", DateTime.UtcNow);
    }

    public ExceptionDto Exception { get; set; }
}