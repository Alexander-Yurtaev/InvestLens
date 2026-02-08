using AutoMapper;
using InvestLens.Data.Core.Abstraction.Repositories;
using InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;
using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;

namespace InvestLens.Data.Api.Services;

public class MoexDataReaderService : IMoexDataReaderService
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
    private readonly IMapper _mapper;

    public MoexDataReaderService(
        ISecurityRepository securityRepository,
        IEngineRepository engineRepository,
        IMarketRepository marketRepository,
        IBoardRepository boardRepository,
        IBoardGroupRepository boardGroupRepository,
        IDurationRepository durationRepository,
        ISecurityTypeRepository securityTypeRepository,
        ISecurityGroupRepository securityGroupRepository,
        ISecurityCollectionRepository securityCollectionRepository,
        IMapper mapper)
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
        _mapper = mapper;
    }

    public async Task<SecurityModelWithPagination> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "")
    {
        var securities = await _securityRepository.Get(page, pageSize, sort, filter);
        var result = new SecurityModelWithPagination
        {
            Page = securities.Page,
            PageSize = securities.PageSize,
            TotalPages = securities.TotalPages,
            TotalItems = securities.TotalItems,
            Models = _mapper.Map<List<SecurityModel>>(securities.Entities)
        };
        
        return result;
    }

    public async Task<SecurityWithDetailsModelWithPagination> GetSecuritiesWithDetails(int page, int pageSize, string? sort = "", string? filter = "")
    {
        var typesTask = _securityTypeRepository.GetAll();
        var groupsTask = _securityGroupRepository.GetAll();
        var securitiesTask = _securityRepository.Get(page, pageSize, sort, filter);

        await Task.WhenAll(typesTask, groupsTask, securitiesTask);

        var types = await typesTask;
        var groups = await groupsTask;
        var securities = await securitiesTask;

        var typeDict = types.ToDictionary(k => k.SecurityTypeName.Trim().ToLowerInvariant(), v => v.SecurityTypeTitle);
        var groupDict = groups.ToDictionary(k => k.Name.Trim().ToLowerInvariant(), v => v.Title);

        var result = new SecurityWithDetailsModelWithPagination()
        {
            Page = securities.Page,
            PageSize = securities.PageSize,
            TotalPages = securities.TotalPages,
            TotalItems = securities.TotalItems,
            Models = _mapper.Map<List<SecurityWithDetailsModel>>(securities.Entities)
        };

        foreach (var model in result.Models)
        {
            var typeKey = model.Type.Trim().ToLowerInvariant();
            model.TypeTitle = typeDict.TryGetValue(typeKey, out var typeValue) ? typeValue : model.Type;

            var groupKey = model.Group.Trim().ToLowerInvariant();
            model.GroupTitle = groupDict.TryGetValue(groupKey, out var groupValue) ? groupValue : model.Group;
        }

        return result;
    }

    public async Task<EntitiesWithPagination<EngineEntity>> GetEngines(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _engineRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<MarketEntity>> GetMarkets(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _marketRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<BoardEntity>> GetBoards(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _boardRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<BoardGroupEntity>> GetBoardGroups(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _boardGroupRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<DurationEntity>> GetDurations(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _durationRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<SecurityTypeEntity>> GetSecurityTypes(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityTypeRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<SecurityGroupEntity>> GetSecurityGroups(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityGroupRepository.Get(page, pageSize, sort, filter);
    }

    public async Task<EntitiesWithPagination<SecurityCollectionEntity>> GetSecurityCollections(int page, int pageSize, string? sort = "", string? filter = "")
    {
        return await _securityCollectionRepository.Get(page, pageSize, sort, filter);
    }
}