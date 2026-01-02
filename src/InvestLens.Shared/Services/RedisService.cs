using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Services;

public class RedisService : IRedisService
{
    private readonly IConfiguration _configuration;

    public RedisService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #region Privaet Methods

    private string GetConnectionString()
    {
        var username = _configuration["REDIS_USER"];
        var password = _configuration["REDIS_PASSWORD"];
        var redis_host = _configuration["REDIS_HOST"];
        var redis_timeout = _configuration["REDIS_TIMEOUT"];
        var redis_ssl = _configuration["REDIS_SSL"];
        var redis_allow_admin = _configuration["REDIS_ALLOW_ADMIN"];

        // redis://username:password@localhost:6379/0?connectTimeout=5000&ssl=true&allowAdmin=true
        return $"redis://{username}:{password}@{redis_host}:6379/0?connectTimeout={redis_timeout}&ssl={redis_ssl}&allowAdmin={redis_allow_admin}";
    }

    #endregion Privaet Methods
}