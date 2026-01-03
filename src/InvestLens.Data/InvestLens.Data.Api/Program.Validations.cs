namespace InvestLens.Data.Api;

public static partial class Program
{
    private static void ValidateSettings(IConfiguration configuration)
    {   
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_HOST"], "DB_HOST");
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_NAME"], "DB_NAME");
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_USER"], "DB_USER");
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_PASSWORD"], "DB_PASSWORD");

        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_HOST"], "REDIS_HOST");
        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_USER"], "REDIS_USER");
        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_PASSWORD"], "REDIS_PASSWORD");
        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_TIMEOUT"], "REDIS_TIMEOUT");
        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_SSL"], "REDIS_SSL");
        ArgumentException.ThrowIfNullOrEmpty(configuration["REDIS_ALLOW_ADMIN"], "REDIS_ALLOW_ADMIN");

        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_HOST"], "RABBITMQ_HOST");
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_USER"], "RABBITMQ_USER");
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_PASSWORD"], "RABBITMQ_PASSWORD");
        ArgumentException.ThrowIfNullOrEmpty(configuration["RABBITMQ_VHOST"], "RABBITMQ_VHOST");

        ArgumentException.ThrowIfNullOrEmpty(configuration["POSTGRES_PASSWORD"], "POSTGRES_PASSWORD");
    }
}