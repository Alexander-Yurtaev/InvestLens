using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Enums;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Redis.Models;
using Polly;

namespace InvestLens.Shared.Redis.Services;

public class SecuritiesRefreshStatusService : ISecuritiesRefreshStatusService
{
    private readonly IRedisClient _redisClient;
    private readonly AsyncPolicy _resilientPolicy;

    public SecuritiesRefreshStatusService(IPollyService pollyService, IRedisClient redisClient)
    {
        _resilientPolicy = pollyService.GetRedisResilientPolicy();
        _redisClient = redisClient;
    }

    public async Task<(string, DateTime)> Init()
    {
        var progress = new SecuritiesRefreshProgress(Guid.NewGuid().ToString());
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));

        return (progress.OperationId, progress.StartedAt);
    }

    public async Task<ISecuritiesRefreshProgress> TryGetProgress()
    {
        var progress = await _resilientPolicy.ExecuteAsync(async () => await _redisClient.GetAsync<SecuritiesRefreshProgress>(RedisKeys.SecuritiesRefreshStatusRedisKey));
        if (progress is null)
        {
            await Init();
            progress = await _resilientPolicy.ExecuteAsync(async () => await _redisClient.GetAsync<SecuritiesRefreshProgress>(RedisKeys.SecuritiesRefreshStatusRedisKey));
        }

        if (progress is null)
        {
            throw new InvalidOperationException($"Проблема при получении {nameof(SecuritiesRefreshProgress)}.");
        }

        return progress;
    }

    public async Task Reset()
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = SecuritiesRefreshStatus.None;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetScheduled()
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = SecuritiesRefreshStatus.Scheduled;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetDownloading(int? count = 0)
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.DownloadedCount = count ?? 0;
        progress.Status = SecuritiesRefreshStatus.Downloading;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetProcessing()
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = SecuritiesRefreshStatus.Processing;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetSaving()
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = SecuritiesRefreshStatus.Saving;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetCompleted(int affected)
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.SavedCount = affected;
        progress.Status = SecuritiesRefreshStatus.Completed;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }

    public async Task SetFailed(string errorMessage)
    {
        var progress = await TryGetProgress();
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Status = SecuritiesRefreshStatus.Failed;
        progress.ErrorMessage = errorMessage;
        await _resilientPolicy.ExecuteAsync(async () => await _redisClient.SetAsync(RedisKeys.SecuritiesRefreshStatusRedisKey, progress));
    }
}