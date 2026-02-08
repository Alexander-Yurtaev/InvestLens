namespace InvestLens.Abstraction.Redis.Services;

public interface IRedisClientFactory
{
    Task<IRedisClient> CreateRedisClient(string instanceName, CancellationToken cancellationToken);
}