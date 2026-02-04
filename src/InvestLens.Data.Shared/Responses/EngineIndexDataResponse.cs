using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class EngineIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("engines")]
    public required Section Section { get; set; }
}