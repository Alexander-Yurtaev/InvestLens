using Grpc.Core;
using InvestLens.Abstraction.Services;
using InvestLens.Grpc.Service;

namespace InvestLens.Data.Api.Services;

public class SecurityGrpcService : SecurityServices.SecurityServicesBase
{
    private readonly IMoexDataService _moexService;
    private readonly ILogger<SecurityGrpcService> _logger;

    public SecurityGrpcService(
        IMoexDataService moexService,
        ILogger<SecurityGrpcService> logger)
    {
        _moexService = moexService;
        _logger = logger;
    }

    public override async Task<GetSecuritiesResponse> GetSecurities(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            var securities = await _moexService.GetSecurities(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecuritiesResponse
            {
                Page = securities.Page,
                PageSize = securities.PageSize,
                TotalPages = securities.TotalPages,
                TotalItems = securities.TotalItems
            };

            response.Data.AddRange(securities.Data.Select(s => new Security
            {
                Id = s.Id,
                SecId = s.SecId,
                ShortName = s.ShortName,
                RegNumber = s.RegNumber, 
                Name = s.Name,
                Isin = s.Isin,
                IsTraded = s.IsTraded,
                EmitentId = s.EmitentId ?? 0,
                EmitentTitle = s.EmitentTitle,
                EmitentInn = s.EmitentInn,
                EmitentOkpo = s.EmitentOkpo,
                Type = s.Type,
                Group = s.Group,
                PrimaryBoardId = s.PrimaryBoardId,
                MarketpriceBoardId = s.MarketpriceBoardId
            }));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<GetSecuritiesWithDetailsResponse> GetSecuritiesWithDetails(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            var securities = await _moexService.GetSecuritiesWithDetails(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecuritiesWithDetailsResponse()
            {
                Page = securities.Page,
                PageSize = securities.PageSize,
                TotalPages = securities.TotalPages,
                TotalItems = securities.TotalItems
            };

            response.Data.AddRange(securities.Data.Select(s => new SecurityWithDetails
            {
                Id = s.Id,
                SecId = s.SecId,
                ShortName = s.ShortName,
                RegNumber = s.RegNumber,
                Name = s.Name,
                Isin = s.Isin,
                IsTraded = s.IsTraded,
                EmitentId = s.EmitentId ?? 0,
                EmitentTitle = s.EmitentTitle,
                EmitentInn = s.EmitentInn,
                EmitentOkpo = s.EmitentOkpo,

                Type = s.Type,
                TypeTitle = s.TypeTitle,
                
                Group = s.Group,
                GroupTitle = s.GroupTitle,

                PrimaryBoardId = s.PrimaryBoardId,
                MarketpriceBoardId = s.MarketpriceBoardId
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