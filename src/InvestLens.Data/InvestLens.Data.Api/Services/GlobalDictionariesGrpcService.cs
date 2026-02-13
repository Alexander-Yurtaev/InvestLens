using AutoMapper;
using Grpc.Core;
using InvestLens.Data.Api.Metrics;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;
using Prometheus;
using Board = InvestLens.Grpc.Service.Board;
using BoardGroup = InvestLens.Grpc.Service.BoardGroup;
using Duration = InvestLens.Grpc.Service.Duration;
using Engine = InvestLens.Grpc.Service.Engine;
using Market = InvestLens.Grpc.Service.Market;
using SecurityCollection = InvestLens.Grpc.Service.SecurityCollection;
using SecurityGroup = InvestLens.Grpc.Service.SecurityGroup;
using SecurityType = InvestLens.Grpc.Service.SecurityType;

namespace InvestLens.Data.Api.Services;

public class GlobalDictionariesGrpcService : GeneralDictionariesServices.GeneralDictionariesServicesBase
{
    private readonly IMoexDataReaderService _moexReaderService;
    private readonly IMapper _mapper;
    private readonly ILogger<GlobalDictionariesGrpcService> _logger;

    public GlobalDictionariesGrpcService(
        IMoexDataReaderService moexReaderService,
        IMapper mapper,
        ILogger<GlobalDictionariesGrpcService> logger)
    {
        _moexReaderService = moexReaderService;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetEnginesResponse> GetEngines(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "Engine")
                .NewTimer();

            var engines = await _moexReaderService.GetEngines(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetEnginesResponse>(engines);
            response.Data.AddRange(_mapper.Map<List<Engine>>(engines.Entities));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetMarketsResponse> GetMarkets(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "Market")
                .NewTimer();

            var markets = await _moexReaderService.GetMarkets(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetMarketsResponse>(markets);
            response.Data.AddRange(_mapper.Map<List<Market>>(markets.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetBoardsResponse> GetBoards(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "Board")
                .NewTimer();

            var boards = await _moexReaderService.GetBoards(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetBoardsResponse>(boards);
            response.Data.AddRange(_mapper.Map<List<Board>>(boards.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetBoardGroupsResponse> GetBoardGroups(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "BoardGroup")
                .NewTimer();

            var boardGroups = await _moexReaderService.GetBoardGroups(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetBoardGroupsResponse>(boardGroups);
            response.Data.AddRange(_mapper.Map<List<BoardGroup>>(boardGroups.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetDurationsResponse> GetDurations(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "Duration")
                .NewTimer();

            var durations = await _moexReaderService.GetDurations(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetDurationsResponse>(durations);
            response.Data.AddRange(_mapper.Map<List<Duration>>(durations.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetSecurityTypesResponse> GetSecurityTypes(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "SecurityType")
                .NewTimer();

            var securityTypes = await _moexReaderService.GetSecurityTypes(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetSecurityTypesResponse>(securityTypes);
            response.Data.AddRange(_mapper.Map<List<SecurityType>>(securityTypes.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetSecurityGroupsResponse> GetSecurityGroups(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "SecurityGroup")
                .NewTimer();

            var securityGroups = await _moexReaderService.GetSecurityGroups(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetSecurityGroupsResponse>(securityGroups);
            response.Data.AddRange(_mapper.Map<List<SecurityGroup>>(securityGroups.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetSecurityCollectionsResponse> GetSecurityCollections(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "SecurityCollection")
                .NewTimer();

            var securityCollections = await _moexReaderService.GetSecurityCollections(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = _mapper.Map<GetSecurityCollectionsResponse>(securityCollections);
            response.Data.AddRange(_mapper.Map<List<SecurityCollection>>(securityCollections.Entities));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}