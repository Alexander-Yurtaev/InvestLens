namespace InvestLens.Abstraction.Services;

public interface ICorrelationIdService
{
    string GetOrCreateCorrelationId(string prefix);
}