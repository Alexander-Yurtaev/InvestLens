using InvestLens.Abstraction.Data;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Data;
using InvestLens.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestLens.Shared.Extensions;

public static class RedisServiceExtensions
{
    public static IServiceCollection AddRedisClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>() ??
                            throw new ArgumentNullException("RedisSettings");

        redisSettings.Username = configuration["REDIS_USER"] ?? "";
        redisSettings.Password = configuration["REDIS_PASSWORD"] ?? "";
        redisSettings.Host = configuration["REDIS_HOST"] ?? "";

        ValidateSettings(redisSettings);

        // Регистрация IRedisSettings
        services.AddSingleton<IRedisSettings>(_ => redisSettings);

        // Регистрация RedisClient
        services.AddSingleton<IRedisClient, RedisClient>(); // общий клиент
        services.AddKeyedSingleton<IRedisClient, RedisClient>(redisSettings.InstanceName); // клиент для сервиса

        // Регистрация распределенного кэша
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.ConnectionString;
            options.InstanceName = redisSettings.InstanceName; // кэш для сервиса
        });

        return services;
    }

    private static void ValidateSettings(RedisSettings redisSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Host, nameof(redisSettings.Host));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Username, nameof(redisSettings.Username));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Password, nameof(redisSettings.Password));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.ConnectionString, nameof(redisSettings.ConnectionString));
    }
}