using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class SecuritiesResponse : IBaseResponse
{
    [JsonPropertyName("securities")]
    public required Section Section { get; set; }
}