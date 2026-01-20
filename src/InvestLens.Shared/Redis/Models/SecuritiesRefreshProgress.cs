using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Enums;

namespace InvestLens.Shared.Redis.Models;

public class SecuritiesRefreshProgress : ISecuritiesRefreshProgress
{
    public SecuritiesRefreshProgress(string operationId)
    {
        OperationId = operationId;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string OperationId { get; set; }

    public SecuritiesRefreshStatus Status { get; set; } = SecuritiesRefreshStatus.None;

    public DateTime StartedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int DownloadedCount { get; set; }

    public int SavedCount { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public TimeSpan Duration => UpdatedAt - StartedAt;

    public void SetStatus(SecuritiesRefreshStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}