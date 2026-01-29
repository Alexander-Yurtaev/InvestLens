using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Shared.Responses;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using InvestLens.Shared.Exceptions;

namespace InvestLens.Shared.Services;

public class MoexClient : IMoexClient
{
    private readonly ILogger<MoexClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly ISecuritiesRefreshStatusService _statusService;
    private readonly IPollyService _pollyService;

    public MoexClient(HttpClient httpClient, ISecuritiesRefreshStatusService statusService, IPollyService pollyService, ILogger<MoexClient> logger)
    {
        _httpClient = httpClient;
        _statusService = statusService;
        _pollyService = pollyService;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <warning>При создании HttpClient были применены AddPolicyHandler.</warning>
    public async Task<SecuritiesResponse?> GetSecurities(string correlationId)
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
        await _statusService.SetDownloading(correlationId);

        while (true)
        {
            SecuritiesResponse? securitiesResponse;
            try
            {
                var resilientPolicy = _pollyService.GetGenericResilientPolicy<string>();
                var jsonString = await resilientPolicy.ExecuteAsync(async () =>
                {
                    using var response = await _httpClient.GetAsync($"/iss/securities.json?start={start}&limit={limit}");
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                });

                securitiesResponse = JsonSerializer.Deserialize<SecuritiesResponse>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                // Обработка HTTP ошибок
                throw new MoexApiException($"MOEX API error: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                // Обработка проблем с соединением
                throw new MoexApiException("Connection error while reading response", ex);
            }

            if (securitiesResponse is null || securitiesResponse.Securities.Data.Length == 0) break;

            if (isFirst)
            {
                result.Securities.Metadata = securitiesResponse.Securities.Metadata;
                result.Securities.Columns = securitiesResponse.Securities.Columns;

                isFirst = false;
            }

            data.AddRange(securitiesResponse.Securities.Data);
            start += limit;

            if (start % 1000 == 0)
            {
                await _statusService.SetDownloading(correlationId, data.Count);
            }
        }

        _logger.LogInformation("От MOEX было получено {SecuritiesCount}.", data.Count);
        await _statusService.SetProcessing(correlationId);

        result.Securities.Data = data.ToArray();
        return result;
    }
}