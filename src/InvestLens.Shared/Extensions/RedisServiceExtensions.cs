using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Shared.Data;
using InvestLens.Shared.Services.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestLens.Shared.Extensions;

public static class RedisServiceExtensions
{
    public static (IServiceCollection, IRedisSettings) AddRedisSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>() ??
                            throw new InvalidOperationException("RedisSettings");

        redisSettings.Username = configuration["REDIS_USER"] ?? "";
        redisSettings.Password = configuration["REDIS_PASSWORD"] ?? "";
        redisSettings.Host = configuration["REDIS_HOST"] ?? "";

        ValidateSettings(redisSettings);

        // Регистрация IRedisSettings
        services.AddSingleton<IRedisSettings>(_ => redisSettings);

        return (services, redisSettings);
    }

    public static IServiceCollection AddRedisClient(this (IServiceCollection, IRedisSettings) owners)
    {
        // Регистрация RedisClient
        owners.Item1.AddSingleton<IRedisClientFactory, RedisClientFactory>();
        owners.Item1.AddSingleton<IRedisClient, LazyRedisClient>(); // общий клиент
        owners.Item1.AddKeyedSingleton<IRedisClient, LazyRedisClientWithInstanceName>(owners.Item2.InstanceName); // клиент для сервиса

        // Регистрация распределенного кэша
        owners.Item1.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = owners.Item2.ConnectionString;
            options.InstanceName = owners.Item2.InstanceName; // кэш для сервиса
        });

        return owners.Item1;
    }

    private static void ValidateSettings(RedisSettings redisSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Host, nameof(redisSettings.Host));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Username, nameof(redisSettings.Username));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.Password, nameof(redisSettings.Password));
        ArgumentException.ThrowIfNullOrEmpty(redisSettings.ConnectionString, nameof(redisSettings.ConnectionString));
    }
}