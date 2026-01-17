using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Services;

public interface ISecurityGrpcClientService
{
    Task<IEnumerable<Security>> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "");
}