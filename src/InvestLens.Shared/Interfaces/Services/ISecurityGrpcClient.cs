using InvestLens.Shared.Models;

namespace InvestLens.Shared.Interfaces.Services;

public interface ISecurityGrpcClient
{
    Task<SecurityModelWithPagination?> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "");

    Task<SecurityWithDetailsModelWithPagination?> GetSecuritiesWithDetailsAsync(int page, int pageSize, string? sort = "",
        string? filter = "");
}