namespace InvestLens.Shared.Contracts.Dto.Entities;

public class BoardDto : DictionaryBaseDto
{
    public int BoardGroupId { get; set; }
    public int EngineId { get; set; }
    public int MarketId { get; set; }
    public string BoardId { get; set; } = string.Empty;
    public string BoardTitle { get; set; } = string.Empty;
    public bool IsTraded { get; set; }
    public bool HasCandles { get; set; }
    public bool IsPrimary { get; set; }
}