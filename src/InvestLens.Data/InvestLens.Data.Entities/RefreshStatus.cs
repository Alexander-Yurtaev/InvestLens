namespace InvestLens.Data.Entities;

public class RefreshStatus
{
    public Guid Id { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public DateTime RefreshDate { get; set; }

    public string LastError { get; set; } = string.Empty;
}