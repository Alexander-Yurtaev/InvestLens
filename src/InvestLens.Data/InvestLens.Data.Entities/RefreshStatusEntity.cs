using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities;

[Table("RefreshStatus")]
public class RefreshStatusEntity : BaseEntity
{
    public RefreshStatusEntity(string entityName)
    {
        EntityName = entityName;
        RefreshDate = DateTime.UtcNow;
    }

    public string EntityName { get; set; }

    public DateTime RefreshDate { get; set; }

    public string LastError { get; set; } = string.Empty;
}