using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Index;

public class Market : IndexBaseEntity
{
    [Column("trade_engine_id")]
    public int TradeEngineId { get; set; }

    [Column("trade_engine_name")]
    [MaxLength(45)]
    public string TradeEngineName { get; set; } = string.Empty;

    [Column("trade_engine_title")]
    [MaxLength(765)]
    public string TradeEngineTitle { get; set; } = string.Empty;

    [Column("market_name")]
    [MaxLength(45)]
    public string MarketName { get; set; } = string.Empty;

    [Column("market_title")]
    [MaxLength(765)]
    public string MarketTitle { get; set; } = string.Empty;

    [Column("market_id")]
    public int MarketId { get; set; }

    [Column("marketplace")]
    [MaxLength(48)]
    public string Marketplace { get; set; } = string.Empty;

    [Column("is_otc")]
    public bool IsOtc { get; set; }

    [Column("has_history_files")]
    public bool HasHistoryFiles { get; set; }

    [Column("has_history_trades_files")]
    public bool HasHistoryTradesFiles { get; set; }

    [Column("has_trades")]
    public bool HasTrades { get; set; }

    [Column("has_history")]
    public bool HasHistory { get; set; }

    [Column("has_candles")]
    public bool HasCandles { get; set; }

    [Column("has_orderbook")]
    public bool HasOrderbook { get; set; }

    [Column("has_tradingsession")]
    public bool HasTradingSession { get; set; }

    [Column("has_extra_yields")]
    public bool HasExtraYields { get; set; }

    [Column("has_delay")]
    public bool HasDelay { get; set; }
}