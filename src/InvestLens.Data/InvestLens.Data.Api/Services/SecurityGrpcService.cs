using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services;

public class SecurityGrpcService : SecurityServices.SecurityServicesBase
{
    private readonly IMoexDataReaderService _moexReaderService;
    private readonly IMapper _mapper;
    private readonly ILogger<SecurityGrpcService> _logger;

    public SecurityGrpcService(
        IMoexDataReaderService moexReaderService,
        IMapper mapper,
        ILogger<SecurityGrpcService> logger)
    {
        _moexReaderService = moexReaderService;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<GetSecuritiesResponse> GetSecurities(GetPaginationRequest request, ServerCallContext context)
    {
        try
        {
            var securities = await _moexReaderService.GetSecurities(request.Page, request.PageSize, request.Sort, request.Filter);
            var response = new GetSecuritiesResponse
            {
                Page = securities.Page,
                PageSize = securities.PageSize,
                TotalPages = securities.TotalPages,
                TotalItems = securities.TotalItems,
            };

            response.Data.AddRange(_mapper.Map<RepeatedField<Security>>(securities.Models));

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
            var securities =
                await _moexReaderService.GetSecuritiesWithDetails(request.Page, request.PageSize, request.Sort,
                    request.Filter);

            var response = new GetSecuritiesWithDetailsResponse()
            {
                Page = securities.Page,
                PageSize = securities.PageSize,
                TotalPages = securities.TotalPages,
                TotalItems = securities.TotalItems
            };

            response.Data.AddRange(securities.Models.Select(s => new SecurityWithDetails
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