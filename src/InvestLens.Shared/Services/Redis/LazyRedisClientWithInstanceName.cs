using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;

namespace InvestLens.Shared.Services.Redis;

public class LazyRedisClientWithInstanceName : BaseLazyRedisClient
{
    public LazyRedisClientWithInstanceName(IRedisClientFactory factory, IRedisSettings settings) : base(settings.InstanceName, factory)
    {
    }
}