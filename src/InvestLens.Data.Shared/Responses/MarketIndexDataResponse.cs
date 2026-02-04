using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class MarketIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("markets")]
    public required Section Section { get; set; }
}