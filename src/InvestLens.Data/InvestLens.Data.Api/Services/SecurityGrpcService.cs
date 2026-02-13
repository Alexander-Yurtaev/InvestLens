using AutoMapper;
using Grpc.Core;
using InvestLens.Data.Api.Metrics;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;
using Prometheus;

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
            var securities = await _moexReaderService
                .GetSecurities(request.Page, request.PageSize, request.Sort, request.Filter);
            
            var response = _mapper.Map<GetSecuritiesResponse>(securities);
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
            using var timer = DataServiceMetrics.DbQueryDuration
                .WithLabels("SELECT", "Security")
                .NewTimer();

            var securities =
                await _moexReaderService.GetSecuritiesWithDetails(request.Page, request.PageSize, request.Sort,
                    request.Filter);

            var response = _mapper.Map<GetSecuritiesWithDetailsResponse>(securities);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}