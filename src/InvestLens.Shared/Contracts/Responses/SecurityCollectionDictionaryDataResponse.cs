using System.Text.Json.Serialization;

namespace InvestLens.Shared.Contracts.Responses;

public class SecurityCollectionDictionaryDataResponse : IBaseDictionaryResponse
{
    [JsonPropertyName("securitycollections")]
    public required Section Section { get; set; }
}