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
    public required string[] Columns { get; set; }

    [JsonPropertyName("metadata")]
    public required Dictionary<string, ColumnMetadata> Metadata { get; set; }

    [JsonPropertyName("data")]
    public required object[][] Data { get; set; }
}

public record ColumnMetadata
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}