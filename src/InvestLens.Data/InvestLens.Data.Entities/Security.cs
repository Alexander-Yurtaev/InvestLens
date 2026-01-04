using System.Text.Json.Serialization;

namespace InvestLens.Data.Entities;

public class Security : BaseEntity<Guid>
{
    [JsonPropertyName("secid")]
    public string SecId { get; set; } = string.Empty;

    [JsonPropertyName("shortname")]
    public string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("regnumber")]
    public string RegNumber { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isin")]
    public string Isin { get; set; } = string.Empty;

    [JsonPropertyName("is_traded")]
    public bool IsTraded { get; set; }

    [JsonPropertyName("emitent_id")]
    public int? EmitentId { get; set; }

    [JsonPropertyName("emitent_title")]
    public string EmitentTitle { get; set; } = string.Empty;

    [JsonPropertyName("emitent_inn")]
    public string EmitentInn { get; set; } = string.Empty;

    [JsonPropertyName("emitent_okpo")]
    public string EmitentOkpo { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("group")]
    public string Group { get; set; } = string.Empty;

    [JsonPropertyName("primary_boardid")]
    public string PrimaryBoardId { get; set; } = string.Empty;

    [JsonPropertyName("marketprice_boardid")]
    public string MarketpriceBoardId { get; set; } = string.Empty;
}