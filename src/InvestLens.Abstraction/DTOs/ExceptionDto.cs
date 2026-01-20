using Google.Protobuf.WellKnownTypes;

namespace InvestLens.Abstraction.DTOs;

public class ExceptionDto (string message, string Tag, DateTime Timestamp)
{
    public string Message { get; } = message;
}