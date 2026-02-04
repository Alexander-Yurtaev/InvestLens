using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class SecurityGroupIndexDataResponse : IBaseIndexResponse
{
    [JsonPropertyName("securitygroups")]
    public required Section Section { get; set; }
}