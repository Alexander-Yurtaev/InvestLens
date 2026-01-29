using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Redis.Services;

public class RedisClientFactory : IRedisClientFactory
{
    private readonly IPollyService _pollyService;
    private readonly IRedisSettings _settings;
    private readonly ILogger<RedisClient> _logger;

    public RedisClientFactory(
        IPollyService pollyService,
        IRedisSettings settings,
        ILogger<RedisClient> logger)
    {
        _pollyService = pollyService;
        _settings = settings;
        _logger = logger;
    }

    public async Task<IRedisClient> CreateRedisClient(string instanceName, CancellationToken cancellationToken)
    {
        var client = await RedisClient.CreateAsync(_pollyService, _settings, instanceName, _logger);
        return client;
    }
}