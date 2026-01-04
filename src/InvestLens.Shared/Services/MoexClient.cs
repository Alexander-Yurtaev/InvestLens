using System.Text.Json;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Shared.Responses;

namespace InvestLens.Shared.Services;

public class MoexClient : IMoexClient
{
    private readonly HttpClient _httpClient;

    public MoexClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("MoexClient");
    }

    public async Task<SecuritiesResponse?> GetSecurities()
    {
        await using var jsonStream = await _httpClient.GetStreamAsync("/iss/securities.json");
        var response = await JsonSerializer.DeserializeAsync<SecuritiesResponse>(jsonStream);
        return response;
    }
}