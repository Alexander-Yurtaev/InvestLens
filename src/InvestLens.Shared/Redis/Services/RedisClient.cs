using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace InvestLens.Shared.Redis.Services;

public class RedisClient : IRedisClient, IDisposable
{
    private readonly IRedisSettings _settings;
    private readonly string _instanceName;
    private readonly ILogger<RedisClient> _logger;
    private IConnectionMultiplexer _connectionMultiplexer = null!;
    private IDatabase _database;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public static async Task<RedisClient> CreateAsync(IRedisSettings settings, string instanceName, ILogger<RedisClient> logger)
    {
        var client = new RedisClient(settings, instanceName, logger);
        await client.InitializeAsync();
        return client;
    }

    internal RedisClient(IRedisSettings settings, string instanceName, ILogger<RedisClient> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _instanceName = instanceName;
        _logger = logger;
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _database = await GetDatabaseAsync();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            var value = await _database.StringGetAsync(redisKey);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get key: {key}", key);
            throw new RedisException($"Failed to get key: {key}", ex);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var redisKey = BuildKey(key);
            var serializedValue = JsonSerializer.Serialize(value);

            await _database.StringSetAsync(redisKey, serializedValue, expiry, When.Always);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set key: {key}", key);
            throw new RedisException($"Failed to set key: {key}", ex);
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.KeyDeleteAsync(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove key: {key}", key);
            throw new RedisException($"Failed to remove key: {key}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.KeyExistsAsync(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence for key: {key}", key);
            throw new RedisException($"Failed to check existence for key: {key}", ex);
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.KeyTimeToLiveAsync(redisKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TTL for key: {key}", key);
            throw new RedisException($"Failed to get TTL for key: {key}", ex);
        }
    }

    public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.KeyExpireAsync(redisKey, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set expiry for key: {key}", key);
            throw new RedisException($"Failed to set expiry for key: {key}", ex);
        }
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern = "*")
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(
                _connectionMultiplexer.GetEndPoints().First());

            var keys = server.Keys(pattern: BuildKey(pattern));
            return await Task.FromResult(keys.Select(k => k.ToString().Replace(_instanceName, "")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get keys with pattern: {pattern}", pattern);
            throw new RedisException($"Failed to get keys with pattern: {pattern}", ex);
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.StringIncrementAsync(redisKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment key: {key}", key);
            throw new RedisException($"Failed to increment key: {key}", ex);
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.StringDecrementAsync(redisKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrement key: {key}", key);
            throw new RedisException($"Failed to decrement key: {key}", ex);
        }
    }

    public async Task<bool> AddToSetAsync(string key, string value)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.SetAddAsync(redisKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add to set: {key}", key);
            throw new RedisException($"Failed to add to set: {key}", ex);
        }
    }

    public async Task<bool> RemoveFromSetAsync(string key, string value)
    {
        try
        {
            var redisKey = BuildKey(key);
            return await _database.SetRemoveAsync(redisKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove from set: {key}", key);
            throw new RedisException($"Failed to remove from set: {key}", ex);
        }
    }

    public async Task<HashSet<string>> GetSetAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            var values = await _database.SetMembersAsync(redisKey);
            return values.Select(v => v.ToString()).ToHashSet();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get set: {key}", key);
            throw new RedisException($"Failed to get set: {key}", ex);
        }
    }

    public async Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var keyArray = keys as string[] ?? keys.ToArray();
            var redisKeys = keyArray.Select(k => (RedisKey)BuildKey(k)).ToArray();
            var values = await _database.StringGetAsync(redisKeys);

            var result = new Dictionary<string, T?>();
            for (int i = 0; i < keyArray.Length; i++)
            {
                var key = keyArray.ElementAt(i);
                var value = values[i];

                result[key] = value.HasValue
                    ? JsonSerializer.Deserialize<T>(value.ToString())
                    : default;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get multiple keys");
            throw new RedisException("Failed to get multiple keys", ex);
        }
    }

    public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null)
    {
        try
        {
            var tasks = keyValuePairs.Select(kvp =>
                SetAsync(kvp.Key, kvp.Value, expiry));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set multiple keys");
            throw new RedisException("Failed to set multiple keys", ex);
        }
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
    {
        try
        {
            var lockKey = $"lock:{BuildKey(key)}";
            var token = Guid.NewGuid().ToString();

            return await _database.StringSetAsync(
                lockKey,
                token,
                expiry,
                When.NotExists,
                CommandFlags.DemandMaster);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire lock for key: {key}", key);
            throw new RedisException($"Failed to acquire lock for key: {key}", ex);
        }
    }

    public async Task ReleaseLockAsync(string key)
    {
        try
        {
            var lockKey = $"lock:{BuildKey(key)}";
            await _database.KeyDeleteAsync(lockKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release lock for key: {key}", key);
            throw new RedisException($"Failed to release lock for key: {key}", ex);
        }
    }

    public async Task<IDatabase> GetDatabaseAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_connectionMultiplexer?.IsConnected != true)
            {
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_settings.ConnectionString);
            }
            _database = _connectionMultiplexer.GetDatabase(_settings.DefaultDatabase);
            return _database;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connectionMultiplexer.Dispose();
            _connectionLock.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    #region Privaet Methods

    private string BuildKey(string key) => $"{_instanceName}{key}";

    #endregion Privaet Methods
}