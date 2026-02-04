using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class SecurityCollectionIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("securitycollections")]
    public required Section Section { get; set; }
}