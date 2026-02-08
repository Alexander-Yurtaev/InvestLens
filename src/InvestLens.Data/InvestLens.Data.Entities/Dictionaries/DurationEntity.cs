using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Dictionaries;

[Table("Duration")]
public class DurationEntity : DictionaryBaseEntity
{
    [Column("interval")]
    public int Interval { get; set; }

    [Column("duration")]
    public int DurationValue { get; set; }

    [Column("days")]
    public int Days { get; set; }

    [Column("title")]
    [MaxLength(765)]
    public string Title { get; set; } = string.Empty;

    [Column("hint")]
    [MaxLength(765)]
    public string Hint { get; set; } = string.Empty;
}