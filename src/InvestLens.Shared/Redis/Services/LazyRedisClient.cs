using InvestLens.Abstraction.Redis.Services;

namespace InvestLens.Shared.Redis.Services;

public class LazyRedisClient : BaseLazyRedisClient
{
    public LazyRedisClient(IRedisClientFactory factory) : base("", factory)
    {
    }
}