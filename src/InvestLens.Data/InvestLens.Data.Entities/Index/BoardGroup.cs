using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Index;

public class BoardGroup : IndexBaseEntity
{
    [Column("trade_engine_id")]
    public int TradeEngineId { get; set; }

    [Column("trade_engine_name")]
    [MaxLength(45)]
    public string TradeEngineName { get; set; } = string.Empty;

    [Column("trade_engine_title")]
    [MaxLength(765)]
    public string TradeEngineTitle { get; set; } = string.Empty;

    [Column("market_id")]
    public int MarketId { get; set; }

    [Column("market_name")]
    [MaxLength(45)]
    public string MarketName { get; set; } = string.Empty;

    [Column("name")]
    [MaxLength(192)]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    [MaxLength(765)]
    public string Title { get; set; } = string.Empty;

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("board_group_id")]
    public int BoardGroupId { get; set; }

    [Column("is_traded")]
    public bool IsTraded { get; set; }

    [Column("is_order_driven")]
    public bool IsOrderDriven { get; set; }

    [Column("category")]
    [MaxLength(45)]
    public string Category { get; set; } = string.Empty;
}