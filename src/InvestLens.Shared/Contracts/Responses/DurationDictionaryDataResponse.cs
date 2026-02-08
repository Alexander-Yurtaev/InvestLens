using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class DurationDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("durations")]
    public required Section Section { get; set; }
}