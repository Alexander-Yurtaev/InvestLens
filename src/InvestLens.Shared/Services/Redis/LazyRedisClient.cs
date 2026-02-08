using InvestLens.Abstraction.Redis.Services;

namespace InvestLens.Shared.Services.Redis;

public class LazyRedisClient : BaseLazyRedisClient
{
    public LazyRedisClient(IRedisClientFactory factory) : base("", factory)
    {
    }
}