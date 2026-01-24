using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Validators;

public static class RabbitMqValidator
{
    public static void Validate(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_USER"], "RABBITMQ_USER");
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_PASSWORD"], "RABBITMQ_PASSWORD");
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_HOST"], "RABBITMQ_HOST");
    }
}