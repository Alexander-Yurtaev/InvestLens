using InvestLens.Shared.Redis.Enums;

namespace InvestLens.Shared.Redis.Models;

public class SecuritiesRefreshState
{
    public SecuritiesRefreshStatus Status { get; set; } = SecuritiesRefreshStatus.None;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public void Reset()
    {
        Status = SecuritiesRefreshStatus.None;
    }

    public void Prepare()
    {
        Status = SecuritiesRefreshStatus.Refresh;
    }

    public void Start()
    {
        Status = SecuritiesRefreshStatus.Refreshing;
    }

    public void Finish()
    {
        FinishedAt = DateTime.UtcNow;
        Status = SecuritiesRefreshStatus.Refreshed;
    }
}