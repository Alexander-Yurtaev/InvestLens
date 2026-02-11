using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Shared.Interfaces.Redis.Data;

namespace InvestLens.Shared.Models.Redis;

public class RefreshProgress : IRefreshProgress
{
    public RefreshProgress(string correlationId)
    {
        CorrelationId = correlationId;
        var now = DateTime.UtcNow;
        StartedAt = now;
        UpdatedAt = now;
    }

    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    
    public string CorrelationId { get; set; }

    public RefreshStatus Status { get; set; } = RefreshStatus.None;

    public DateTime StartedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int DownloadedCount { get; set; }

    public int SavedCount { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public TimeSpan Duration => UpdatedAt - StartedAt;
}