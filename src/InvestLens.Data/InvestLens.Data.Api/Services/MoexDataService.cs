using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Index;

namespace InvestLens.Data.Api.Services;

public class MoexDataService : IMoexDataService
{
    private readonly ISecurityRepository _securityRepository;
    private readonly IEngineRepository _engineRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardGroupRepository _boardGroupRepository;
    private readonly IDurationRepository _durationRepository;
    private readonly ISecurityTypeRepository _securityTypeRepository;
    private readonly ISecurityGroupRepository _securityGroupRepository;
    private readonly ISecurityCollectionRepository _securityCollectionRepository;

    public MoexDataService(
        ISecurityRepository securityRepository,
        IEngineRepository engineRepository,
        IMarketRepository marketRepository,
        IBoardRepository boardRepository,
        IBoardGroupRepository boardGroupRepository,
        IDurationRepository durationRepository,
        ISecurityTypeRepository securityTypeRepository,
        ISecurityGroupRepository securityGroupRepository,
        ISecurityCollectionRepository securityCollectionRepository)
    {
        _securityRepository = securityRepository;
        _engineRepository = engineRepository;
        _marketRepository = marketRepository;
        _boardRepository = boardRepository;
        _boardGroupRepository = boardGroupRepository;
        _durationRepository = durationRepository;
        _securityTypeRepository = securityTypeRepository;
        _securityGroupRepository = securityGroupRepository;
        _securityCollectionRepository = securityCollectionRepository;
    }

    public async Task<IGetResult<Security>> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<Engine>> GetEngines(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _engineRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<Market>> GetMarkets(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _marketRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<Board>> GetBoards(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _boardRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<BoardGroup>> GetBoardGroups(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _boardGroupRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<Duration>> GetDurations(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _durationRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<SecurityType>> GetSecurityTypes(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityTypeRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<SecurityGroup>> GetSecurityGroups(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityGroupRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<IGetResult<SecurityCollection>> GetSecurityCollections(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityCollectionRepository.Get(page, pageSize, sort, filter);
    }
}