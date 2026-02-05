using Grpc.Core;
using InvestLens.Abstraction.Services;
using InvestLens.Grpc.Service;
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
    private readonly IMoexDataService _moexService;
    private readonly ILogger<GlobalDictionariesGrpcService> _logger;

    public GlobalDictionariesGrpcService(
        IMoexDataService moexService,
        ILogger<GlobalDictionariesGrpcService> logger)
    {
        _moexService = moexService;
        _logger = logger;
    }

    public override async Task<GetEnginesResponse> GetEngines(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            var engines = await _moexService.GetEngines(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetEnginesResponse
            {
                Page = engines.Page,
                PageSize = engines.PageSize,
                TotalPages = engines.TotalPages,
                TotalItems = engines.TotalItems
            };

            response.Data.AddRange(engines.Data.Select(e => new Engine
            {
                Id = e.Id,
                Name = e.Name,
                Title = e.Title
            }));

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
            var markets = await _moexService.GetMarkets(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetMarketsResponse
            {
                Page = markets.Page,
                PageSize = markets.PageSize,
                TotalPages = markets.TotalPages,
                TotalItems = markets.TotalItems
            };

            response.Data.AddRange(markets.Data.Select(m => new Market
            {
                Id = m.Id,
                TradeEngineId = m.TradeEngineId,
                TradeEngineName = m.TradeEngineName,
                TradeEngineTitle = m.TradeEngineTitle,
                MarketName = m.MarketName,
                MarketTitle = m.MarketTitle,
                MarketId = m.MarketId,
                Marketplace = m.Marketplace,
                IsOtc = m.IsOtc,
                HasHistoryFiles = m.HasHistoryFiles,
                HasHistoryTradesFiles = m.HasHistoryTradesFiles,
                HasTrades = m.HasTrades,
                HasHistory = m.HasHistory,
                HasCandles = m.HasCandles,
                HasOrderbook = m.HasOrderbook,
                HasTradingSession = m.HasTradingSession,
                HasExtraYields = m.HasExtraYields,
                HasDelay = m.HasDelay
            }));

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
            var boards = await _moexService.GetBoards(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetBoardsResponse
            {
                Page = boards.Page,
                PageSize = boards.PageSize,
                TotalPages = boards.TotalPages,
                TotalItems = boards.TotalItems
            };

            response.Data.AddRange(boards.Data.Select(b => new Board
            {
                Id = b.Id,
                BoardGroupId = b.BoardGroupId,
                EngineId = b.EngineId,
                MarketId = b.MarketId,
                BoardId = b.BoardId,
                BoardTitle = b.BoardTitle,
                IsTraded = b.IsTraded,
                HasCandles = b.HasCandles,
                IsPrimary = b.IsPrimary
            }));

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
            var boardGroups = await _moexService.GetBoardGroups(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetBoardGroupsResponse
            {
                Page = boardGroups.Page,
                PageSize = boardGroups.PageSize,
                TotalPages = boardGroups.TotalPages,
                TotalItems = boardGroups.TotalItems
            };

            response.Data.AddRange(boardGroups.Data.Select(bg => new BoardGroup
            {
                Id = bg.Id,
                TradeEngineId = bg.TradeEngineId,
                TradeEngineName = bg.TradeEngineName,
                TradeEngineTitle = bg.TradeEngineTitle,
                MarketId = bg.MarketId,
                MarketName = bg.MarketName,
                Name = bg.Name,
                Title = bg.Title,
                IsDefault = bg.IsDefault,
                BoardGroupId = bg.BoardGroupId,
                IsTraded = bg.IsTraded,
                IsOrderDriven = bg.IsOrderDriven,
                Category = bg.Category
            }));

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
            var durations = await _moexService.GetDurations(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetDurationsResponse
            {
                Page = durations.Page,
                PageSize = durations.PageSize,
                TotalPages = durations.TotalPages,
                TotalItems = durations.TotalItems
            };

            response.Data.AddRange(durations.Data.Select(d => new Duration
            {
                Interval = d.Interval,
                DurationValue = d.DurationValue,
                Days = d.Days,
                Title = d.Title,
                Hint = d.Hint
            }));

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
            var securityTypes = await _moexService.GetSecurityTypes(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecurityTypesResponse
            {
                Page = securityTypes.Page,
                PageSize = securityTypes.PageSize,
                TotalPages = securityTypes.TotalPages,
                TotalItems = securityTypes.TotalItems
            };

            response.Data.AddRange(securityTypes.Data.Select(st => new SecurityType
            {
                Id = st.Id,
                TradeEngineId = st.TradeEngineId,
                TradeEngineName = st.TradeEngineName,
                TradeEngineTitle = st.TradeEngineTitle,
                SecurityTypeName = st.SecurityTypeName,
                SecurityTypeTitle = st.SecurityTypeTitle,
                SecurityGroupName = st.SecurityGroupName,
                StockType = st.StockType
            }));

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
            var securityGroups = await _moexService.GetSecurityGroups(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecurityGroupsResponse
            {
                Page = securityGroups.Page,
                PageSize = securityGroups.PageSize,
                TotalPages = securityGroups.TotalPages,
                TotalItems = securityGroups.TotalItems
            };

            response.Data.AddRange(securityGroups.Data.Select(sg => new SecurityGroup
            {
                Id = sg.Id,
                Name = sg.Name,
                Title = sg.Title,
                IsHidden = sg.IsHidden
            }));

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
            var securityCollections = await _moexService.GetSecurityCollections(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecurityCollectionsResponse
            {
                Page = securityCollections.Page,
                PageSize = securityCollections.PageSize,
                TotalPages = securityCollections.TotalPages,
                TotalItems = securityCollections.TotalItems
            };

            response.Data.AddRange(securityCollections.Data.Select(sg => new SecurityCollection
            {
                Id = sg.Id,
                Name = sg.Name,
                Title = sg.Title,
                SecurityGroupId = sg.SecurityGroupId
            }));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}