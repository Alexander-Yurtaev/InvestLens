using InvestLens.Abstraction.DTOs;

namespace InvestLens.Abstraction.Services;

public interface ISecurityGrpcClientService
{
    Task<SecurityDto?> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "");

    Task<SecurityWithDetailsDto?> GetSecuritiesWithDetailsAsync(int page, int pageSize, string? sort = "",
        string? filter = "");
}