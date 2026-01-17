using InvestLens.Abstraction.DTOs;

namespace InvestLens.Abstraction.Services;

public interface ISecurityGrpcClientService
{
    Task<SecuritiesDto> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "");
}