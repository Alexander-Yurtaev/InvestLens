using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Index;

public class Engine : IndexBaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;
}