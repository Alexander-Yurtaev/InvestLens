using Grpc.Core;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Securities.Service;

namespace InvestLens.Data.Api.Services;

public class SecurityGrpcService : SecurityServices.SecurityServicesBase
{
    private readonly IDataService _dataService;
    private readonly ILogger<SecurityGrpcService> _logger;

    public SecurityGrpcService(
        IDataService dataService,
        ILogger<SecurityGrpcService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public override async Task<GetSecuritiesResponse> GetSecurities(GetSecuritiesRequest request, ServerCallContext context)
    {
        try
        {
            var securities = await _dataService.GetSecurities(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecuritiesResponse
            {
                Page = securities.Page,
                PageSize = securities.PageSize,
                TotalPages = securities.TotalPages,
                TotlaItems = securities.TotalItems
            };

            response.Data.AddRange(securities.Data.Select(s => new Security
            {
                Id = s.Id.ToString(),
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
}