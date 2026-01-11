using InvestLens.Abstraction.Redis.Services;

namespace InvestLens.Shared.Redis.Services;

public abstract class BaseLazyRedisClient : IRedisClient
{
    private readonly Lazy<Task<IRedisClient>> _lazyClient;

    protected BaseLazyRedisClient(string instanceName, IRedisClientFactory factory)
    {
        _lazyClient = new Lazy<Task<IRedisClient>>(() => factory.CreateRedisClient(instanceName, CancellationToken.None));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var client = await _lazyClient.Value;
        return await client.GetAsync<T>(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var client = await _lazyClient.Value;
        await client.SetAsync(key, value, expiry);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        var client = await _lazyClient.Value;
        return await client.RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var client = await _lazyClient.Value;
        return await client.ExistsAsync(key);
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        var client = await _lazyClient.Value;
        return await client.GetTimeToLiveAsync(key);
    }

    public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
    {
        var client = await _lazyClient.Value;
        return await client.SetExpiryAsync(key, expiry);
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern = "*")
    {
        var client = await _lazyClient.Value;
        return await client.GetKeysAsync(pattern);
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        var client = await _lazyClient.Value;
        return await client.IncrementAsync(key, value);
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        var client = await _lazyClient.Value;
        return await client.DecrementAsync(key, value);
    }

    public async Task<bool> AddToSetAsync(string key, string value)
    {
        var client = await _lazyClient.Value;
        return await client.AddToSetAsync(key, value);
    }

    public async Task<bool> RemoveFromSetAsync(string key, string value)
    {
        var client = await _lazyClient.Value;
        return await client.RemoveFromSetAsync(key, value);
    }

    public async Task<HashSet<string>> GetSetAsync(string key)
    {
        var client = await _lazyClient.Value;
        return await client.GetSetAsync(key);
    }

    public async Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        var client = await _lazyClient.Value;
        return await client.GetMultipleAsync<T>(keys);
    }

    public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null)
    {
        var client = await _lazyClient.Value;
        await client.SetMultipleAsync(keyValuePairs, expiry);
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
    {
        var client = await _lazyClient.Value;
        return await client.AcquireLockAsync(key, expiry);
    }

    public async Task ReleaseLockAsync(string key)
    {
        var client = await _lazyClient.Value;
        await client.ReleaseLockAsync(key);
    }
}