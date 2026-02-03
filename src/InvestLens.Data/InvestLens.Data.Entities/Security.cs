using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities;

public class Security : BaseEntity
{
    [DisplayName("ID")]
    [Column("secid")]
    public string SecId { get; set; } = string.Empty;

    [DisplayName("Кор. название")]
    [Column("shortname")]
    public string ShortName { get; set; } = string.Empty;

    [DisplayName("Рег. номер")]
    [Column("regnumber")]
    public string? RegNumber { get; set; } = string.Empty;

    [DisplayName("Название")]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("ИСИН")]
    [Column("isin")]
    public string? Isin { get; set; } = string.Empty;

    [DisplayName("Торгуется")]
    [Column("is_traded")]
    public bool IsTraded { get; set; }

    [DisplayName("ID эмитента")]
    [Column("emitent_id")]
    public int? EmitentId { get; set; }

    [DisplayName("Эмитент")]
    [Column("emitent_title")]
    public string? EmitentTitle { get; set; } = string.Empty;

    [DisplayName("ИНН эмитента")]
    [Column("emitent_inn")]
    public string? EmitentInn { get; set; } = string.Empty;

    [DisplayName("ОКПО эмитента")]
    [Column("emitent_okpo")]
    public string? EmitentOkpo { get; set; } = string.Empty;

    [DisplayName("Тип")]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [DisplayName("Группа")]
    [Column("group")]
    public string Group { get; set; } = string.Empty;

    [DisplayName("Главная площадка")]
    [Column("primary_boardid")]
    public string? PrimaryBoardId { get; set; } = string.Empty;

    [DisplayName("ID торг. площадки")]
    [Column("marketprice_boardid")]
    public string? MarketpriceBoardId { get; set; } = string.Empty;
}