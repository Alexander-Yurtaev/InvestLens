using System.Text.Json.Serialization;
using InvestLens.Shared.Contracts.Dto;

namespace InvestLens.Shared.Messages;

public class ErrorMessage : BaseTelegramMessage
{
    private string _innerExceptionMessage = string.Empty;

    public ErrorMessage(DateTime? finishedAt, string innerExceptionMessage)
    {
        FinishedAt = finishedAt;
        InnerExceptionMessage = innerExceptionMessage;
    }

    public string InnerExceptionMessage
    {
        get => _innerExceptionMessage;
        set
        {
            _innerExceptionMessage = value;
            Exception = new ExceptionDto(
                string.IsNullOrEmpty(_innerExceptionMessage) ? "Произошла ошибка" : _innerExceptionMessage,
                "SECURITY_REFRESH_ERROR", DateTime.UtcNow);
        }
    }

    [JsonIgnore]
    public ExceptionDto Exception { get; set; } = null!;
}