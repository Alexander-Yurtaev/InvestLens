using AutoMapper;
using Grpc.Net.Client;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;
using InvestLens.Shared.Models.Dictionaries;

namespace InvestLens.Gateway.Services;

public class GlobalDictionariesGrpcClient : IEngineDictionariesGrpcClient, 
                                            IMarketDictionariesGrpcClient,
                                            IBoardDictionariesGrpcClient,
                                            IBoardGroupDictionariesGrpcClient,
                                            IDurationDictionariesGrpcClient,
                                            ISecurityTypeDictionariesGrpcClient,
                                            ISecurityGroupDictionariesGrpcClient,
                                            ISecurityCollectionDictionariesGrpcClient
{
    private readonly string _grpcServiceAddress;
    private readonly IMapper _mapper;
    private readonly ILogger<GlobalDictionariesGrpcClient> _logger;

    public GlobalDictionariesGrpcClient(IConfiguration configuration, IMapper mapper, ILogger<GlobalDictionariesGrpcClient> logger)
    {
        var grpcServiceAddress = configuration["GrpcMoexServerAddress"];
        ArgumentException.ThrowIfNullOrEmpty(grpcServiceAddress, "GrpcMoexServerAddress");
        _grpcServiceAddress = grpcServiceAddress;

        _mapper = mapper;
        _logger = logger;
    }

    async Task<BaseModelWithPagination<EngineModel>?> IBaseDictionariesGrpcClient<EngineModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new EngineModelWithPagination();
            var response = await client.GetEnginesAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(engine => _mapper.Map<EngineModel>(engine)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<MarketModel>?> IBaseDictionariesGrpcClient<MarketModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new MarketModelWithPagination();
            var response = await client.GetMarketsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(market => _mapper.Map<MarketModel>(market)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<BoardModel>?> IBaseDictionariesGrpcClient<BoardModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new BoardModelWithPagination();
            var response = await client.GetBoardsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(board => _mapper.Map<BoardModel>(board)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<BoardGroupModel>?> IBaseDictionariesGrpcClient<BoardGroupModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new BoardGroupModelWithPagination();
            var response = await client.GetBoardGroupsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(boardGroup => _mapper.Map<BoardGroupModel>(boardGroup)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<DurationModel>?> IBaseDictionariesGrpcClient<DurationModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new DurationModelWithPagination();
            var response = await client.GetDurationsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(duration => _mapper.Map<DurationModel>(duration)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<SecurityTypeModel>?> IBaseDictionariesGrpcClient<SecurityTypeModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityTypeModelWithPagination();
            var response = await client.GetSecurityTypesAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(securityType => _mapper.Map<SecurityTypeModel>(securityType)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<SecurityGroupModel>?> IBaseDictionariesGrpcClient<SecurityGroupModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityGroupModelWithPagination();
            var response = await client.GetSecurityGroupsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(securityGroup => _mapper.Map<SecurityGroupModel>(securityGroup)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    async Task<BaseModelWithPagination<SecurityCollectionModel>?> IBaseDictionariesGrpcClient<SecurityCollectionModel>.GetEntitiesAsync(int page, int pageSize, string? sort, string? filter)
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new GeneralDictionariesServices.GeneralDictionariesServicesClient(channel);

            var result = new SecurityCollectionModelWithPagination();
            var response = await client.GetSecurityCollectionsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(securityCollection => _mapper.Map<SecurityCollectionModel>(securityCollection)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}