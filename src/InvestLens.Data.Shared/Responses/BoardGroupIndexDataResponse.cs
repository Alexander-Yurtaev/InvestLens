using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class BoardGroupIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("boardgroups")]
    public required Section Section { get; set; }
}