using InvestLens.Data.Shared.Responses;

namespace InvestLens.Abstraction.Services;

public interface IMoexClient
{
    Task<SecuritiesResponse?> GetSecurities(string operationId);
}