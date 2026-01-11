using InvestLens.Shared.Redis.Enums;

namespace InvestLens.Shared.Redis.Models;

public record SecuritiesRefreshState
{
    public SecuritiesRefreshStatus Status { get; init; } = SecuritiesRefreshStatus.None;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }
}