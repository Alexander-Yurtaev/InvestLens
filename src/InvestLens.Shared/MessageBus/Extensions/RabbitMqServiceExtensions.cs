using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.MessageBus.Data;
using InvestLens.Shared.MessageBus.Services;
using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestLens.Shared.MessageBus.Extensions;

public static class RabbitMqServiceExtensions
{
    public static IServiceCollection AddRabbitMqClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RabbitMqValidator.Validate(configuration);

        var rabbitMqSettings = configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>() ??
                            throw new ArgumentNullException("RabbitMqSettings");

        rabbitMqSettings.UserName = configuration["RABBITMQ_USER"]!;
        rabbitMqSettings.Password = configuration["RABBITMQ_PASSWORD"]!;
        rabbitMqSettings.HostName = configuration["RABBITMQ_HOST"]!;

        ValidateSettings(rabbitMqSettings);

        // Регистрация IRedisSettings
        services.AddSingleton<IRabbitMqSettings>(_ => rabbitMqSettings);

        // Регистрация RabbitMqClient
        services.AddSingleton<IRabbitMqClientFactory, RabbitMqClientFactory>();
        services.AddSingleton<IMessageBusClient, LazyRabbitMqClient>();

        return services;
    }

    private static void ValidateSettings(IRabbitMqSettings rabbitMqSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.HostName, nameof(rabbitMqSettings.HostName));
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.UserName, nameof(rabbitMqSettings.UserName));
        ArgumentException.ThrowIfNullOrEmpty(rabbitMqSettings.Password, nameof(rabbitMqSettings.Password));
    }
}