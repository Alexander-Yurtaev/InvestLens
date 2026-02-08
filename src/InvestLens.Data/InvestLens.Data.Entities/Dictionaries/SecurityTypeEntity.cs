using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Dictionaries;

[Table("SecurityType")]
public class SecurityTypeEntity : DictionaryBaseEntity
{
    [Column("trade_engine_id")]
    public int TradeEngineId { get; set; }

    [Column("trade_engine_name")]
    [MaxLength(43)]
    public string TradeEngineName { get; set; } = string.Empty;

    [Column("trade_engine_title")]
    [MaxLength(765)]
    public string TradeEngineTitle { get; set; } = string.Empty;

    [Column("security_type_name")]
    [MaxLength(93)]
    public string SecurityTypeName { get; set; } = string.Empty;

    [Column("security_type_title")]
    [MaxLength(765)]
    public string SecurityTypeTitle { get; set; } = string.Empty;

    [Column("security_group_name")]
    [MaxLength(93)]
    public string SecurityGroupName { get; set; } = string.Empty;

    [Column("stock_type")]
    [MaxLength(3)]
    public string StockType { get; set; } = string.Empty;
}