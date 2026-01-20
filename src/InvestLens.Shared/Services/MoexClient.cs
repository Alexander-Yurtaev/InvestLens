using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Shared.Responses;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InvestLens.Shared.Services;

public class MoexClient : IMoexClient
{
    private readonly ILogger<MoexClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly ISecuritiesRefreshStatusService _statusService;

    public MoexClient(HttpClient httpClient, ISecuritiesRefreshStatusService statusService, ILogger<MoexClient> logger)
    {
        _httpClient = httpClient;
        _statusService = statusService;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <warning>При создании HttpClient были применены AddPolicyHandler.</warning>
    public async Task<SecuritiesResponse?> GetSecurities(string operationId)
    {
        var start = 0;
        const int limit = 100;
        var isFirst = true;

        var data = new List<object[]>();
        var result = new SecuritiesResponse
        {
            Securities = new Securities
            {
                Metadata = new Dictionary<string, ColumnMetadata>(),
                Columns = [],
                Data = []
            }
        };

        _logger.LogInformation("Начало получения данных от MOEX.");
        await _statusService.SetDownloading();

        while (true)
        {
            await using var jsonStream = await _httpClient.GetStreamAsync($"/iss/securities.json?start={start}&limit={limit}");
            var response = await JsonSerializer.DeserializeAsync<SecuritiesResponse>(jsonStream);

            if (response is null || response.Securities.Data.Length == 0) break;

            if (isFirst)
            {
                result.Securities.Metadata = response.Securities.Metadata;
                result.Securities.Columns = response.Securities.Columns;

                isFirst = false;
            }

            data.AddRange(response.Securities.Data);
            start += limit;

            if (start % 1000 == 0)
            {
                await _statusService.SetDownloading(data.Count);
            }
        }

        _logger.LogInformation("От MOEX было получено {SecuritiesCount}.", data.Count);
        await _statusService.SetProcessing();

        result.Securities.Data = data.ToArray();
        return result;
    }
}