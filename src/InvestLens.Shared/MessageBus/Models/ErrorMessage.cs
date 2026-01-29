using InvestLens.Abstraction.DTOs;

namespace InvestLens.Shared.MessageBus.Models;

public class ErrorMessage : BaseTelegramMessage
{
    public ErrorMessage(DateTime? finishedAt, Exception? exception)
    {
        FinishedAt = finishedAt;
        Exception = new ExceptionDto(exception?.Message ?? "Произошла ошибка", "SECURITY_REFRESH_ERROR", DateTime.UtcNow);
    }

    public ExceptionDto Exception { get; set; }
}