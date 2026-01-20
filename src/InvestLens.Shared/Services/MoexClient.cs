using InvestLens.Abstraction.Services;
using InvestLens.Data.Shared.Responses;
using InvestLens.Shared.Constants;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Data.Shared.Redis;

namespace InvestLens.Shared.Services;

public class MoexClient : IMoexClient
{
    private readonly ILogger<MoexClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IRedisClient _redisClient;

    public MoexClient(HttpClient httpClient, IRedisClient redisClient, ILogger<MoexClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _redisClient = redisClient;
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

        await ClearStatusAsync();

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

            if (start % 1000 == 0)
            {
                await SaveStatusAsync(DateTime.UtcNow - startDate, data.Count);
            }
        }

        var finishDate = DateTime.UtcNow;
        _logger.LogInformation("От MOEX было полуено {SecuritiesCount} за {Seconds} сек.", data.Count, (finishDate-startDate).Seconds);

        await ClearStatusAsync();

        result.Securities.Data = data.ToArray();
        return result;
    }

    private async Task SaveStatusAsync(TimeSpan duration, int count)
    {
        var jobStatus = new JobStatus("Обновление Securities.", $"Загружено {count} записей за {duration:hh\\:mm\\:ss}.");
        await _redisClient.SetAsync(RedisKeys.JobStatusKey, jobStatus);
    }

    private async Task ClearStatusAsync()
    {
        await _redisClient.RemoveAsync(RedisKeys.JobStatusKey);
    }
}