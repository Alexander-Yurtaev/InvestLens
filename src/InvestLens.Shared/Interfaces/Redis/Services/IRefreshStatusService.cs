using InvestLens.Shared.Interfaces.Redis.Data;

namespace InvestLens.Shared.Interfaces.Redis.Services;

public interface IRefreshStatusService
{
    Task<DateTime> Init(string correlationId);
    Task<IRefreshProgress> TryGetProgress(string correlationId);
    Task Reset(string correlationId);
    Task SetScheduled(string correlationId);
    Task SetDownloading(string correlationId, int count);
    Task SetProcessing(string correlationId);
    Task SetSaving(string correlationId, int count);
    Task SetCompleted(string correlationId, int affected);
    Task SetFailed(string correlationId, string errorMessage);
}