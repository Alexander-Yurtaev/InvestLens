using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Data;
using InvestLens.Shared.Interfaces.MessageBus.Services;
using InvestLens.Shared.Services.RabbitMq;
using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestLens.Shared.Extensions;

public static class RabbitMqServiceExtensions
{
    public static (IServiceCollection, IRabbitMqSettings) AddRabbitMqSettings(this IServiceCollection services, IConfiguration configuration)
    {
        RabbitMqValidator.Validate(configuration);

        var rabbitMqSettings = configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>() ??
                               throw new InvalidOperationException("RabbitMqSettings");

        rabbitMqSettings.UserName = configuration["RABBITMQ_USER"]!;
        rabbitMqSettings.Password = configuration["RABBITMQ_PASSWORD"]!;
        rabbitMqSettings.HostName = configuration["RABBITMQ_HOST"]!;

        ValidateSettings(rabbitMqSettings);

        // Регистрация IRedisSettings
        services.AddSingleton<IRabbitMqSettings>(_ => rabbitMqSettings);

        return (services, rabbitMqSettings);
    }

    public static IServiceCollection AddRabbitMqClient(this (IServiceCollection, IRabbitMqSettings) owners)
    {
        // Регистрация RabbitMqClient
        owners.Item1.AddSingleton<IRabbitMqClientFactory, RabbitMqClientFactory>();
        owners.Item1.AddSingleton<IMessageBusClient, LazyRabbitMqClient>();

        return owners.Item1;
    }

    private static void ValidateSettings(RabbitMqSettings rabbitMqSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.HostName, nameof(rabbitMqSettings.HostName));
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.UserName, nameof(rabbitMqSettings.UserName));
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.Password, nameof(rabbitMqSettings.Password));
    }
}