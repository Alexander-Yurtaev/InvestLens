using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class BoardIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("boards")]
    public required Section Section { get; set; }
}