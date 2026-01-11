using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;

namespace InvestLens.Shared.Redis.Services;

public class LazyRedisClientWithInstanceName : BaseLazyRedisClient
{
    public LazyRedisClientWithInstanceName(IRedisClientFactory factory, IRedisSettings settings) : base(settings.InstanceName, factory)
    {
    }
}