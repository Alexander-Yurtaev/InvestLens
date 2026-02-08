using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public record ColumnMetadata
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}