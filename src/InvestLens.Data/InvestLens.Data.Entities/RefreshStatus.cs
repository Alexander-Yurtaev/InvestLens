namespace InvestLens.Data.Entities;

public class RefreshStatus : BaseEntity
{
    public RefreshStatus(string entityName)
    {
        EntityName = entityName;
        RefreshDate = DateTime.UtcNow;
    }

    public string EntityName { get; set; }

    public DateTime RefreshDate { get; set; }

    public string LastError { get; set; } = string.Empty;
}