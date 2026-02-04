using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class DurationIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("durations")]
    public required Section Section { get; set; }
}