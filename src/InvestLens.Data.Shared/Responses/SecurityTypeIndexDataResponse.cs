using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class SecurityTypeIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("securitytypes")]
    public required Section Section { get; set; }
}