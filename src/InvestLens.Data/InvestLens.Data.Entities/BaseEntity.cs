using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities;

public abstract class BaseEntity
{
    [Column("id")]
    public int Id { get; set; }
}