using System.ComponentModel;
using System.Text.Json.Serialization;

namespace InvestLens.Data.Entities;

public class Security : BaseEntity<Guid>
{
    [DisplayName("ID")]
    [JsonPropertyName("secid")]
    public string SecId { get; set; } = string.Empty;

    [DisplayName("Кор. название")]
    [JsonPropertyName("shortname")]
    public string ShortName { get; set; } = string.Empty;

    [DisplayName("Рег. номер")]
    [JsonPropertyName("regnumber")]
    public string? RegNumber { get; set; } = string.Empty;

    [DisplayName("Название")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("ИСИН")]
    [JsonPropertyName("isin")]
    public string? Isin { get; set; } = string.Empty;

    [DisplayName("Торгуется")]
    [JsonPropertyName("is_traded")]
    public bool IsTraded { get; set; }

    [DisplayName("ID эмитента")]
    [JsonPropertyName("emitent_id")]
    public int? EmitentId { get; set; }

    [DisplayName("Эмитент")]
    [JsonPropertyName("emitent_title")]
    public string? EmitentTitle { get; set; } = string.Empty;

    [DisplayName("ИНН эмитента")]
    [JsonPropertyName("emitent_inn")]
    public string? EmitentInn { get; set; } = string.Empty;

    [DisplayName("ОКПО эмитента")]
    [JsonPropertyName("emitent_okpo")]
    public string? EmitentOkpo { get; set; } = string.Empty;

    [DisplayName("Тип")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [DisplayName("Группа")]
    [JsonPropertyName("group")]
    public string Group { get; set; } = string.Empty;

    [DisplayName("Главная площадка")]
    [JsonPropertyName("primary_boardid")]
    public string? PrimaryBoardId { get; set; } = string.Empty;

    [DisplayName("ID торг. площадки")]
    [JsonPropertyName("marketprice_boardid")]
    public string? MarketpriceBoardId { get; set; } = string.Empty;
}