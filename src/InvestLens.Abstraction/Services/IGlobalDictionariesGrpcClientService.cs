using InvestLens.Abstraction.DTOs;

namespace InvestLens.Abstraction.Services;

public interface IGlobalDictionariesGrpcClientService
{
    Task<EngineDto?> GetEnginesAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<MarketDto?> GetMarketsAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<BoardDto?> GetBoardsAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<BoardGroupDto?> GetBoardGroupsAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<DurationDto?> GetDurationsAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<SecurityTypeDto?> GetSecurityTypesAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<SecurityGroupDto?> GetSecurityGroupsAsync(int page, int pageSize, string? sort = "", string? filter = "");
    Task<SecurityCollectionDto?> GetSecurityCollectionsAsync(int page, int pageSize, string? sort = "", string? filter = "");
}