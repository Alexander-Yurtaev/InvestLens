using InvestLens.Abstraction.MessageBus.Services;

namespace InvestLens.Abstraction.Redis.Services;

public interface IRedisClientFactory
{
    Task<IRedisClient> CreateRedisClient(string instanceName, CancellationToken cancellationToken);
}