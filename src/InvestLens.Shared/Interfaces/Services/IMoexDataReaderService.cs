using InvestLens.Data.Entities;
using InvestLens.Data.Entities.Dictionaries;
using InvestLens.Shared.Models;

namespace InvestLens.Shared.Interfaces.Services;

public interface IMoexDataReaderService
{
    Task<SecurityModelWithPagination> GetSecurities(int page, int pageSize, string? sort = "",
        string? filter = "");

    Task<SecurityWithDetailsModelWithPagination> GetSecuritiesWithDetails(int page, int pageSize, string? sort = "",
        string? filter = "");

    Task<EntitiesWithPagination<EngineEntity>> GetEngines(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<MarketEntity>> GetMarkets(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<BoardEntity>> GetBoards(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<BoardGroupEntity>> GetBoardGroups(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<DurationEntity>> GetDurations(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<SecurityTypeEntity>> GetSecurityTypes(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<SecurityGroupEntity>> GetSecurityGroups(int page, int pageSize, string? sort = "", string? filter = "");
    Task<EntitiesWithPagination<SecurityCollectionEntity>> GetSecurityCollections(int page, int pageSize, string? sort = "", string? filter = "");

    Task<IEnumerable<SecurityTypeEntity>> GetAllSecurityTypes();
    Task<IEnumerable<SecurityGroupEntity>> GetAllSecurityGroups();
}