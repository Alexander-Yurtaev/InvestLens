using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class SecurityGroupDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("securitygroups")]
    public required Section Section { get; set; }
}