using InvestLens.Abstraction.Redis.Data;

namespace InvestLens.Abstraction.Redis.Services;

public interface ISecuritiesRefreshStatusService
{
    Task<(Guid, DateTime)> Init();
    Task<ISecuritiesRefreshProgress> TryGetProgress();
    Task Reset();
    Task SetScheduled();
    Task SetDownloading(int? count = 0);
    Task SetProcessing();
    Task SetSaving();
    Task SetCompleted(int affected);
    Task SetFailed(string errorMessage);
}