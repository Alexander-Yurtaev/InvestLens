using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

[JsonDerivedType(typeof(SecuritiesResponse))]
public abstract class BaseResponse : IBaseResponse
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    [JsonIgnore]
    public Dictionary<string, Section> Sections
    {
        get
        {
            var sections = new Dictionary<string, Section>();

            if (ExtensionData != null)
            {
                foreach (var kvp in ExtensionData)
                {
                    try
                    {
                        var section = JsonSerializer.Deserialize<Section>(
                            kvp.Value.GetRawText(),
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        if (section != null)
                        {
                            sections[kvp.Key] = section;
                        }
                    }
                    catch
                    {
                        // Пропускаем секции, которые нельзя десериализовать
                    }
                }
            }

            return sections;
        }
    }
}

public class Section
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}