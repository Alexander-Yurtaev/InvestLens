namespace InvestLens.Shared.Models;

public class SecurityModel : BaseModel
{
    public string SecId { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string? RegNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Isin { get; set; } = string.Empty;
    public bool IsTraded { get; set; }
    public int? EmitentId { get; set; }
    public string? EmitentTitle { get; set; } = string.Empty;
    public string? EmitentInn { get; set; } = string.Empty;
    public string? EmitentOkpo { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string? PrimaryBoardId { get; set; } = string.Empty;
    public string? MarketpriceBoardId { get; set; } = string.Empty;
}