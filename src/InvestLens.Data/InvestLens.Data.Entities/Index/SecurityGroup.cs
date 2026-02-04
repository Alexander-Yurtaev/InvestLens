using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Index;

public class SecurityGroup : IndexBaseEntity
{
    [Column("name")]
    [MaxLength(93)]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    [MaxLength(765)]
    public string Title { get; set; } = string.Empty;

    [Column("is_hidden")]
    public bool IsHidden { get; set; }
}