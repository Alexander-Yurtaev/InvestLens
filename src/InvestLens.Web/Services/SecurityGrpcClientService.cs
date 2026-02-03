using Grpc.Net.Client;
using InvestLens.Abstraction.DTOs;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Securities.Service;
using Security = InvestLens.Data.Entities.Security;

namespace InvestLens.Web.Services;

public class SecurityGrpcClientService : ISecurityGrpcClientService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityGrpcClientService> _logger;

    public SecurityGrpcClientService(IConfiguration configuration, ILogger<SecurityGrpcClientService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SecuritiesDto?> GetSecuritiesAsync(int page, int pageSize, string? sort = "", string? filter = "")
    {
        try
        {
            var grpcServiceAddress = _configuration["GrpcSecuritiesServerAddress"];

            ArgumentException.ThrowIfNullOrEmpty(grpcServiceAddress, "GrpcSecuritiesServerAddress");

            using var channel = GrpcChannel.ForAddress(grpcServiceAddress);
            var client = new SecurityServices.SecurityServicesClient(channel);

            var result = new SecuritiesDto();
            var response = await client.GetSecuritiesAsync(new GetSecuritiesRequest { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });
            result.Page = response.Page;
            result.PageSize = response.PageSize;
            result.TotalPages = response.TotalPages;
            result.TotalItems = response.TotlaItems;
            result.Data = response.Data.Select(s => new Security
            {
                Id = int.Parse(s.Id),
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
}