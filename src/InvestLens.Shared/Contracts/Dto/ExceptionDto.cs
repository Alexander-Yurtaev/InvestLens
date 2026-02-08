namespace InvestLens.Shared.Contracts.Dto;

public class ExceptionDto(string message, string tag, DateTime timestamp)
{
    public string Message { get; } = message;
    public string Tag { get; } = tag;
    public DateTime Timestamp { get; } = timestamp;
}