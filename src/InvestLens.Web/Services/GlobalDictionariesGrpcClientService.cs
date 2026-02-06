using AutoMapper;
using Grpc.Net.Client;
using InvestLens.Abstraction.DTOs;
using InvestLens.Abstraction.Services;
using InvestLens.Grpc.Service;
using Board = InvestLens.Data.Entities.Index.Board;
using BoardGroup = InvestLens.Data.Entities.Index.BoardGroup;
using Duration = InvestLens.Data.Entities.Index.Duration;
using Engine = InvestLens.Data.Entities.Index.Engine;
using Market = InvestLens.Data.Entities.Index.Market;
using SecurityType = InvestLens.Data.Entities.Index.SecurityType;
using SecurityGroup = InvestLens.Data.Entities.Index.SecurityGroup;
using SecurityCollection = InvestLens.Data.Entities.Index.SecurityCollection;

namespace InvestLens.Web.Services;

public class GlobalDictionariesGrpcClientService : IEngineDictionariesGrpcClientService, 
                                                   IMarketDictionariesGrpcClientService,
                                                   IBoardDictionariesGrpcClientService,
                                                   IBoardGroupDictionariesGrpcClientService,
                                                   IDurationDictionariesGrpcClientService,
                                                   ISecurityTypeDictionariesGrpcClientService,
                                                   ISecurityGroupDictionariesGrpcClientService,
                                                   ISecurityCollectionDictionariesGrpcClientService
{
    private readonly string _grpcServiceAddress;
    private readonly IMapper _mapper;
    private readonly ILogger<GlobalDictionariesGrpcClientService> _logger;

    public GlobalDictionariesGrpcClientService(IConfiguration configuration, IMapper mapper, ILogger<GlobalDictionariesGrpcClientService> logger)
    {
        var grpcServiceAddress = configuration["GrpcMoexServerAddress"];
        ArgumentException.ThrowIfNullOrEmpty(grpcServiceAddress, "GrpcMoexServerAddress");
        _grpcServiceAddress = grpcServiceAddress;

        _mapper = mapper;
        _logger = logger;
    }

    async Task<BaseEntityDto<Engine>?> IBaseDictionariesGrpcClientService<Engine>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new EngineDto();
            var response = await client.GetEnginesAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(engine => _mapper.Map<Engine>(engine)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<Market>?> IBaseDictionariesGrpcClientService<Market>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new MarketDto();
            var response = await client.GetMarketsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(market => _mapper.Map<Market>(market)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<Board>?> IBaseDictionariesGrpcClientService<Board>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new BoardDto();
            var response = await client.GetBoardsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(board => _mapper.Map<Board>(board)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<BoardGroup>?> IBaseDictionariesGrpcClientService<BoardGroup>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new BoardGroupDto();
            var response = await client.GetBoardGroupsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(boardGroup => _mapper.Map<BoardGroup>(boardGroup)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<Duration>?> IBaseDictionariesGrpcClientService<Duration>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new DurationDto();
            var response = await client.GetDurationsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(duration => _mapper.Map<Duration>(duration)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<SecurityType>?> IBaseDictionariesGrpcClientService<SecurityType>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityTypeDto();
            var response = await client.GetSecurityTypesAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(securityType => _mapper.Map<SecurityType>(securityType)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<SecurityGroup>?> IBaseDictionariesGrpcClientService<SecurityGroup>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityGroupDto();
            var response = await client.GetSecurityGroupsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(securityGroup => _mapper.Map<SecurityGroup>(securityGroup)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseEntityDto<SecurityCollection>?> IBaseDictionariesGrpcClientService<SecurityCollection>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityCollectionDto();
            var response = await client.GetSecurityCollectionsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Data = response.Data.Select(securityCollection => _mapper.Map<SecurityCollection>(securityCollection)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}