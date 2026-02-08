using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class MarketDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("markets")]
    public required Section Section { get; set; }
}