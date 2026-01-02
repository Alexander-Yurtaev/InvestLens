using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace InvestLens.Shared.Services;

public class PostgresDataService : IDataService
{
    public string GetMasterConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["POSTGRES_DATA_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = "postgres",
            Username = "postgres",
            Password = configuration["POSTGRES_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;

    }

    public string GetTargetMasterConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["POSTGRES_DATA_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = GetDatabaseName(configuration),
            Username = "postgres",
            Password = configuration["POSTGRES_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;
    }

    public string GetTargetConnectionString(IConfiguration configuration)
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = configuration["POSTGRES_DATA_HOST"],
            Port = 5432, // порт по умолчанию для PostgreSQL
            Database = GetDatabaseName(configuration),
            Username = configuration["POSTGRES_DATA_USER"],
            Password = configuration["POSTGRES_DATA_PASSWORD"],
            CommandTimeout = 30 // таймаут выполнения команд
        };

        return builder.ConnectionString;
    }

    public string GetDatabaseName(IConfiguration configuration)
    {
        var dbName = configuration["DB_NAME"];

        ArgumentException.ThrowIfNullOrEmpty(dbName);

        return dbName;
    }
}
