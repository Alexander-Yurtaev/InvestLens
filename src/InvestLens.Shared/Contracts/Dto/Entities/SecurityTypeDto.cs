namespace InvestLens.Shared.Contracts.Dto.Entities;

public class SecurityTypeDto : DictionaryBaseDto
{
    public int TradeEngineId { get; set; }
    public string TradeEngineName { get; set; } = string.Empty;
    public string TradeEngineTitle { get; set; } = string.Empty;
    public string SecurityTypeName { get; set; } = string.Empty;
    public string SecurityTypeTitle { get; set; } = string.Empty;
    public string SecurityGroupName { get; set; } = string.Empty;
    public string StockType { get; set; } = string.Empty;
}