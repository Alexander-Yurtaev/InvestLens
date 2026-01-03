using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InvestLens.Shared.Helpers;

public static class PostgresDataHelper
{
    public static string GetMasterConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["DB_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = "postgres",
            Username = "postgres",
            Password = configuration["POSTGRES_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;

    }

    public static string GetTargetMasterConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["DB_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = GetDatabaseName(configuration),
            Username = "postgres",
            Password = configuration["POSTGRES_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;
    }

    public static string GetTargetConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["DB_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = GetDatabaseName(configuration),
            Username = configuration["DB_USER"],
            Password = configuration["DB_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;
    }

    public static string GetDatabaseName(IConfiguration configuration)
    {
        var dbName = configuration["DB_NAME"];

        ArgumentException.ThrowIfNullOrEmpty(dbName);

        return dbName;
    }

    public static string? GetTargetMigration(IConfiguration configuration)
    {
        var targetMigration = configuration["TARGET_MIGRATION"];

        ArgumentException.ThrowIfNullOrEmpty(targetMigration);

        return targetMigration;
    }

    public static (string username, string password) GetServiceUserInfo(IConfiguration configuration)
    {
        var username = configuration["DB_USER"];
        var password = configuration["DB_PASSWORD"];

        ArgumentException.ThrowIfNullOrEmpty(username, "DB_USER");
        ArgumentException.ThrowIfNullOrEmpty(password, "DB_PASSWORD");

        return (username, password);
    }
}
