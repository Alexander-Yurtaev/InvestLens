using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Dictionaries;

[Table("Engine")]
public class EngineEntity : DictionaryBaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;
}