using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class BoardGroupDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("boardgroups")]
    public required Section Section { get; set; }
}