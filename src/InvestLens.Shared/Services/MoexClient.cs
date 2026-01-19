using InvestLens.Abstraction.Services;
using InvestLens.Data.Shared.Responses;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Services;

public class MoexClient : IMoexClient
{
    private readonly ILogger<MoexClient> _logger;
    private readonly HttpClient _httpClient;

    public MoexClient(HttpClient httpClient, ILogger<MoexClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <warning>При создании HttpClient были применены AddPolicyHandler.</warning>
    public async Task<SecuritiesResponse?> GetSecurities()
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

        // https://iss.moex.com/iss/securities.json?start=10000&limit=100
        var startDate = DateTime.UtcNow;
        _logger.LogInformation("Начало получения данных от MOEX.");

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
        }

        var finishDate = DateTime.UtcNow;
        _logger.LogInformation("От MOEX было полуено {SecuritiesCount} за {Seconds} сек.", data.Count, (finishDate-startDate).Seconds);

        result.Securities.Data = data.ToArray();
        return result;
    }
}