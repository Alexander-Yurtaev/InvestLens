using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public record ColumnMetadata
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}