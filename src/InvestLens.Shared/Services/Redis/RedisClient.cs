using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Shared.Services.Redis;

public class RedisClient : IRedisClient, IDisposable
{
    private readonly IPollyService _pollyService;
    private readonly IRedisSettings _settings;
    private readonly string _instanceName;
    private readonly ILogger<RedisClient> _logger;
    private IConnectionMultiplexer? _connectionMultiplexer;
    private IDatabase _database = null!;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public static async Task<RedisClient> CreateAsync(IPollyService pollyService, IRedisSettings settings,
        string instanceName, ILogger<RedisClient> logger)
    {
        var client = new RedisClient(pollyService, settings, instanceName, logger);
        await client.InitializeAsync();
        return client;
    }

    internal RedisClient(IPollyService pollyService, IRedisSettings settings, string instanceName, ILogger<RedisClient> logger)
    {
        _pollyService = pollyService;
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
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            var redisKey = BuildKey(key);
            //await resilientPolicy.ExecuteAsync(async () => await RemoveAsync(redisKey));
            var value = await resilientPolicy.ExecuteAsync(async () => await _database.StringGetAsync(redisKey));

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

            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();

            await resilientPolicy.ExecuteAsync(async () => await _database.StringSetAsync(redisKey, serializedValue, expiry, When.Always));
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
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();

            return await resilientPolicy.ExecuteAsync(async () => await _database.KeyDeleteAsync(redisKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove key: {key}", key);
            throw new RedisException($"Failed to remove key: {key}", ex);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var redisKey = BuildKey(key);
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            return await resilientPolicy.ExecuteAsync(async (_, _) => await _database.KeyExistsAsync(redisKey), [], cancellationToken: cancellationToken);
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
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            return await resilientPolicy.ExecuteAsync(async () => await _database.KeyTimeToLiveAsync(redisKey));
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
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            return await resilientPolicy.ExecuteAsync(async () => await _database.KeyExpireAsync(redisKey, expiry));
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
            await InitConnectionMultiplexer();
            var server = _connectionMultiplexer!.GetServer(_connectionMultiplexer.GetEndPoints().First());

            var keys = server.Keys(pattern: BuildKey(pattern));
            return await Task.FromResult(keys.Select(k =>
            {
                var v = k.ToString();
                return !v.StartsWith(_instanceName, StringComparison.OrdinalIgnoreCase) ? v : v.Substring(_instanceName.Length);
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get keys with pattern: {pattern}", pattern);
            throw new RedisException($"Failed to get keys with pattern: {pattern}", ex);
        }
    }

    //public async Task<long> IncrementAsync(string key, long value = 1)
    //{
    //    try
    //    {
    //        var redisKey = BuildKey(key);
    //        return await _database.StringIncrementAsync(redisKey, value);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to increment key: {key}", key);
    //        throw new RedisException($"Failed to increment key: {key}", ex);
    //    }
    //}

    //public async Task<long> DecrementAsync(string key, long value = 1)
    //{
    //    try
    //    {
    //        var redisKey = BuildKey(key);
    //        return await _database.StringDecrementAsync(redisKey, value);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to decrement key: {key}", key);
    //        throw new RedisException($"Failed to decrement key: {key}", ex);
    //    }
    //}

    //public async Task<bool> AddToSetAsync(string key, string value)
    //{
    //    try
    //    {
    //        var redisKey = BuildKey(key);
    //        return await _database.SetAddAsync(redisKey, value);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to add to set: {key}", key);
    //        throw new RedisException($"Failed to add to set: {key}", ex);
    //    }
    //}

    //public async Task<bool> RemoveFromSetAsync(string key, string value)
    //{
    //    try
    //    {
    //        var redisKey = BuildKey(key);
    //        return await _database.SetRemoveAsync(redisKey, value);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to remove from set: {key}", key);
    //        throw new RedisException($"Failed to remove from set: {key}", ex);
    //    }
    //}

    public async Task<HashSet<string>> GetSetAsync(string key)
    {
        try
        {
            var redisKey = BuildKey(key);
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            return await resilientPolicy.ExecuteAsync(async () =>
            {
                var values = await _database.SetMembersAsync(redisKey);
                return values.Select(v => v.ToString()).ToHashSet();
            });
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
            var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
            var values = await resilientPolicy.ExecuteAsync(async () => await _database.StringGetAsync(redisKeys));

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

    //public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null)
    //{
    //    try
    //    {
    //        var tasks = keyValuePairs.Select(kvp =>
    //            SetAsync(kvp.Key, kvp.Value, expiry));

    //        await Task.WhenAll(tasks);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to set multiple keys");
    //        throw new RedisException("Failed to set multiple keys", ex);
    //    }
    //}

    //public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
    //{
    //    try
    //    {
    //        var lockKey = $"lock:{BuildKey(key)}";
    //        var token = Guid.NewGuid().ToString();

    //        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
    //        return await resilientPolicy.ExecuteAsync(async () => await _database.StringSetAsync(
    //            lockKey,
    //            token,
    //            expiry,
    //            When.NotExists,
    //            CommandFlags.DemandMaster));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to acquire lock for key: {key}", key);
    //        throw new RedisException($"Failed to acquire lock for key: {key}", ex);
    //    }
    //}

    //public async Task ReleaseLockAsync(string key)
    //{
    //    try
    //    {
    //        var resilientPolicy = _pollyService.GetResilientPolicy<Exception>();
    //        await resilientPolicy.ExecuteAsync(async () =>
    //        {
    //            var lockKey = $"lock:{BuildKey(key)}";
    //            return await _database.KeyDeleteAsync(lockKey);
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to release lock for key: {key}", key);
    //        throw new RedisException($"Failed to release lock for key: {key}", ex);
    //    }
    //}

    public async Task<IDatabase> GetDatabaseAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            await InitConnectionMultiplexer();
            _database = _connectionMultiplexer!.GetDatabase(_settings.DefaultDatabase);
            return _database;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task InitConnectionMultiplexer()
    {
        if (_connectionMultiplexer is not { IsConnected: true })
        {
            _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_settings.ConnectionString);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connectionMultiplexer?.Dispose();
            _connectionLock.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    #region Private Methods

    private string BuildKey(string key) => $"{_instanceName}{key}";

    #endregion Private Methods
}