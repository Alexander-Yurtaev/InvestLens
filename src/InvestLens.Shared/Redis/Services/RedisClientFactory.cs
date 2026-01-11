using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Redis.Services;

public class RedisClientFactory : IRedisClientFactory
{
    private readonly IRedisSettings _settings;
    private readonly ILogger<RedisClient> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RedisClientFactory(IRedisSettings settings,
        ILogger<RedisClient> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<IRedisClient> CreateRedisClient(string instanceName, CancellationToken cancellationToken)
    {
        var client = await RedisClient.CreateAsync(_settings, instanceName, _logger);
        return client;
    }
}