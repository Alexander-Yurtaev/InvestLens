using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class BoardDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("boards")]
    public required Section Section { get; set; }
}