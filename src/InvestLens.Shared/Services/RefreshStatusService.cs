using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Redis.Data;
using InvestLens.Shared.Interfaces.Redis.Services;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models.Redis;
using Polly;

namespace InvestLens.Shared.Services;

public class RefreshStatusService : IRefreshStatusService
{
    private readonly IRedisClient _redisClient;
    private readonly AsyncPolicy _resilientPolicy;

    public RefreshStatusService(IPollyService pollyService, IRedisClient redisClient)
    {
        _resilientPolicy = pollyService.GetRedisResilientPolicy();
        _redisClient = redisClient;
    }

    public async Task<DateTime> Init(string correlationId)
    {
        var progress = new RefreshProgress(correlationId);
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));

        return progress.StartedAt;
    }

    public async Task<IRefreshProgress> TryGetProgress(string correlationId)
    {
        var progress = await _resilientPolicy.ExecuteAsync(async () => await _redisClient.GetAsync<RefreshProgress>(RedisKeys.SecuritiesRefreshStatusRedisKey));
        if (progress is null)
        {
            await Init(correlationId);
            progress = await _resilientPolicy.ExecuteAsync(async () => await _redisClient.GetAsync<RefreshProgress>(RedisKeys.SecuritiesRefreshStatusRedisKey));
        }

        return progress ?? throw new InvalidOperationException($"Problem with receiving {nameof(RefreshProgress)}.");
    }

    public async Task Reset(string correlationId)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = RefreshStatus.None;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetScheduled(string correlationId)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = RefreshStatus.Scheduled;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetDownloading(string correlationId, int count)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.DownloadedCount = count;
        progress.Status = RefreshStatus.Processing;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetProcessing(string correlationId)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = RefreshStatus.Processing;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetSaving(string correlationId, int count)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.SavedCount = count;
        progress.Status = RefreshStatus.Processing;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetCompleted(string correlationId, int affected)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.SavedCount = affected;
        progress.Status = RefreshStatus.Completed;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetFailed(string correlationId, string errorMessage)
    {
        var progress = await TryGetProgress(correlationId);
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = RefreshStatus.Failed;
        progress.ErrorMessage = errorMessage;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }
}