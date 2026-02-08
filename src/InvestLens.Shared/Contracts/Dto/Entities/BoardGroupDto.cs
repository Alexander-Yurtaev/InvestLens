namespace InvestLens.Shared.Contracts.Dto.Entities;

public class BoardGroupDto : DictionaryBaseDto
{
    public int TradeEngineId { get; set; }
    public string TradeEngineName { get; set; } = string.Empty;
    public string TradeEngineTitle { get; set; } = string.Empty;
    public int MarketId { get; set; }
    public string MarketName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int BoardGroupId { get; set; }
    public bool IsTraded { get; set; }
    public bool IsOrderDriven { get; set; }
    public string Category { get; set; } = string.Empty;
}