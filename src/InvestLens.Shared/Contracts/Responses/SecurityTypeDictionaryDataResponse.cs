using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class SecurityTypeDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("securitytypes")]
    public required Section Section { get; set; }
}