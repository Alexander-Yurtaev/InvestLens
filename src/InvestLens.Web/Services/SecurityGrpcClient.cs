using Grpc.Net.Client;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;

namespace InvestLens.Web.Services;

public class SecurityGrpcClient : ISecurityGrpcClient
{
    private readonly string _grpcServiceAddress;
    private readonly ILogger<SecurityGrpcClient> _logger;
    
    public SecurityGrpcClient(IConfiguration configuration, ILogger<SecurityGrpcClient> logger)
    {
        var grpcServiceAddress = configuration["GrpcMoexServerAddress"];
        ArgumentException.ThrowIfNullOrEmpty(grpcServiceAddress, "GrpcMoexServerAddress");
        _grpcServiceAddress = grpcServiceAddress;

        _logger = logger;
    }

    public async Task<SecurityModelWithPagination?> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_grpcServiceAddress);
            var client = new SecurityServices.SecurityServicesClient(channel);

            var result = new SecurityModelWithPagination();
            var response = await client.GetSecuritiesAsync(new GetPaginationRequest() { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(s => new SecurityModel
            {
                Id = s.Id,
                SecId = s.SecId ?? string.Empty,
                ShortName = s.ShortName ?? string.Empty,
                RegNumber = s.RegNumber ?? string.Empty,
                Name = s.Name ?? string.Empty,
                Isin = s.Isin ?? string.Empty,
                IsTraded = s.IsTraded ?? false,
                EmitentId = s.EmitentId ?? 0,
                EmitentTitle = s.EmitentTitle ?? string.Empty,
                EmitentInn = s.EmitentInn ?? string.Empty,
                EmitentOkpo = s.EmitentOkpo ?? string.Empty,
                Type = s.Type ?? string.Empty,
                Group = s.Group ?? string.Empty,
                PrimaryBoardId = s.PrimaryBoardId ?? string.Empty,
                MarketpriceBoardId = s.MarketpriceBoardId ?? string.Empty
            }).ToList();

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

            var result = new SecurityWithDetailsModelWithPagination();
            var response = await client.GetSecuritiesWithDetailsAsync(new GetPaginationRequest() { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotalItems;
            result.Models = response.Data.Select(s => new SecurityWithDetailsModel
            {
                Id = s.Id,
                SecId = s.SecId ?? string.Empty,
                ShortName = s.ShortName ?? string.Empty,
                RegNumber = s.RegNumber ?? string.Empty,
                Name = s.Name ?? string.Empty,
                Isin = s.Isin ?? string.Empty,
                IsTraded = s.IsTraded ?? false,
                EmitentId = s.EmitentId ?? 0,
                EmitentTitle = s.EmitentTitle ?? string.Empty,
                EmitentInn = s.EmitentInn ?? string.Empty,
                EmitentOkpo = s.EmitentOkpo ?? string.Empty,
                Type = s.Type ?? string.Empty,
                TypeTitle = s.TypeTitle ?? string.Empty,
                Group = s.Group ?? string.Empty,
                GroupTitle = s.GroupTitle ?? string.Empty,
                PrimaryBoardId = s.PrimaryBoardId ?? string.Empty,
                MarketpriceBoardId = s.MarketpriceBoardId ?? string.Empty
            }).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}