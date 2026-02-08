using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Dictionaries;

[Table("SecurityCollection")]
public class SecurityCollectionEntity : DictionaryBaseEntity
{
    [Column("name")]
    [MaxLength(96)]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    [MaxLength(765)]
    public string Title { get; set; } = string.Empty;

    [Column("security_group_id")]
    public int SecurityGroupId { get; set; }
}