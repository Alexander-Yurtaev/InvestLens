using AutoMapper;
using Grpc.Net.Client;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;

namespace InvestLens.Gateway.Services;

public class SecurityGrpcClient : ISecurityGrpcClient
{
    private readonly string _grpcServiceAddress;
    private readonly IMapper _mapper;
    private readonly ILogger<SecurityGrpcClient> _logger;
    
    public SecurityGrpcClient(IConfiguration configuration, IMapper mapper, ILogger<SecurityGrpcClient> logger)
    {
        var grpcServiceAddress = configuration["GrpcMoexServerAddress"];
        ArgumentException.ThrowIfNullOrEmpty(grpcServiceAddress, "GrpcMoexServerAddress");
        _grpcServiceAddress = grpcServiceAddress;

        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityModelWithPagination?> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new SecurityServices.SecurityServicesClient(channel);

            var response = await client.GetSecuritiesAsync(new GetPaginationRequest() { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            var result = _mapper.Map<SecurityModelWithPagination>(response);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }

    public async Task<SecurityWithDetailsModelWithPagination?> GetSecuritiesWithDetailsAsync(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new SecurityServices.SecurityServicesClient(channel);

            var response = await client.GetSecuritiesWithDetailsAsync(new GetPaginationRequest()
                { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            var result = _mapper.Map<SecurityWithDetailsModelWithPagination>(response);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}