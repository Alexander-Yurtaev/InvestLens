namespace InvestLens.Abstraction.Services;

public interface IRedisClient
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<TimeSpan?> GetTimeToLiveAsync(string key);
    Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
    Task<IEnumerable<string>> GetKeysAsync(string pattern = "*");
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
    Task<bool> AddToSetAsync(string key, string value);
    Task<bool> RemoveFromSetAsync(string key, string value);
    Task<HashSet<string>> GetSetAsync(string key);
    Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);
    Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null);
    Task<bool> AcquireLockAsync(string key, TimeSpan expiry);
    Task ReleaseLockAsync(string key);
}