namespace InvestLens.Shared.Interfaces.Services;

public interface ICorrelationIdService
{
    string GetOrCreateCorrelationId(string prefix);
    void SetCorrelationId(string correlationId);
}