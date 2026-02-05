using InvestLens.Abstraction.Repositories;
using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Index;

namespace InvestLens.Abstraction.Services;

public interface IMoexDataService
{
    Task<IGetResult<Security>> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "");

    Task<IGetResult<Engine>> GetEngines(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<Market>> GetMarkets(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<Board>> GetBoards(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<BoardGroup>> GetBoardGroups(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<Duration>> GetDurations(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<SecurityType>> GetSecurityTypes(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<SecurityGroup>> GetSecurityGroups(int page, int pageSize, string? sort = "", string? filter = "");
    Task<IGetResult<SecurityCollection>> GetSecurityCollections(int page, int pageSize, string? sort = "", string? filter = "");
}