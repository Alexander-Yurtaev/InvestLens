using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;

namespace InvestLens.Data.Api.Services;

public class SecurityDataService : ISecurityDataService
{
    private readonly ISecurityRepository _securityRepository;
    private readonly ILogger<SecurityDataService> _logger;

    public SecurityDataService(
        ISecurityRepository securityRepository,
        ILogger<SecurityDataService> logger)
    {
        _securityRepository = securityRepository;
        _logger = logger;
    }

    public async Task<IGetResult<Security>> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityRepository.Get(page, pageSize, sort, filter);
    }
}