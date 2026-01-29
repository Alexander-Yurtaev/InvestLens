using InvestLens.Abstraction.Redis.Enums;

namespace InvestLens.Abstraction.Redis.Data;

public interface ISecuritiesRefreshProgress
{
    string MessageId { get; set; }

    string CorrelationId { get; set; }
    DateTime StartedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    SecuritiesRefreshStatus Status { get; set; }
    int DownloadedCount { get; set; }
    int SavedCount { get; set; }
    TimeSpan Duration { get; }
    string ErrorMessage { get; set; }
}