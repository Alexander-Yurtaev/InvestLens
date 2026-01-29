namespace InvestLens.Shared.Exceptions;

public class MoexApiException : Exception
{
    public MoexApiException(string message) : base(message) { }
    public MoexApiException(string message, Exception innerException)
        : base(message, innerException) { }
}