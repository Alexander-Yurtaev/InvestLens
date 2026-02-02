using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public record SecuritiesResponse
{
    [JsonPropertyName("securities")]
    public required Securities Securities { get; set; }
}

public record Securities
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public record ColumnMetadata
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}