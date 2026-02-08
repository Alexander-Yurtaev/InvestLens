using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class EngineDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("engines")]
    public required Section Section { get; set; }
}