using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class Section
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}