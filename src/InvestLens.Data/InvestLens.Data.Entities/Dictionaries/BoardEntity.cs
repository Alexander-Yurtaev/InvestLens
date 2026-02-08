using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Dictionaries;

[Table("Board")]
public class BoardEntity : DictionaryBaseEntity
{
    [Column("board_group_id")]
    public int BoardGroupId { get; set; }

    [Column("engine_id")]
    public int EngineId { get; set; }

    [Column("market_id")]
    public int MarketId { get; set; }

    [Column("boardid")]
    [MaxLength(12)]
    public string BoardId { get; set; } = string.Empty;

    [Column("board_title")]
    [MaxLength(381)]
    public string BoardTitle { get; set; } = string.Empty;

    [Column("is_traded")]
    public bool IsTraded { get; set; }

    [Column("has_candles")]
    public bool HasCandles { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }
}