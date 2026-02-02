using InvestLens.Abstraction.Services;
using InvestLens.Shared.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Serilog;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;
using System.Reflection;

namespace InvestLens.Shared.Helpers;

public static class DatabaseHelper
{
    #region Create&Migrations

    public static async Task EnsureDatabaseInitAsync(WebApplication app)
    {
        CommonValidator.CommonValidate(app.Configuration);

        Log.Information("Starting database initialization...");

        try
        {
            using var scope = app.Services.CreateScope();
            var pollyService = scope.ServiceProvider.GetService<IPollyService>()!;

            // 1. Ждем доступность PostgreSQL
            await WaitForPostgresAsync(app.Configuration);

            // 2. Создаем БД и пользователя
            await EnsureDatabaseCreatedAsync(pollyService, app.Configuration, true);

            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization failed");
            throw;
        }
    }

    public static async Task WaitForPostgresAsync(IConfiguration configuration)
    {
        var masterConnectionString = ConnectionStringHelper.GetMasterConnectionString(configuration);

        var waitPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount: 40, // До 40 попыток (~3-4 минуты)
                sleepDurationProvider: _ => TimeSpan.FromSeconds(5),
                onRetry: (exception, retryAttempt, _, _) =>
                {
                    Log.Warning(
                        "Waiting for PostgreSQL ({RetryAttempt}/40): {Message}",
                        retryAttempt, exception.Message);
                });

        Log.Information("Waiting for PostgreSQL to become available...");

        await waitPolicy.ExecuteAsync(async () =>
        {
            await using var connection = new NpgsqlConnection(masterConnectionString);
            await connection.OpenAsync();

            // Проверяем, что PostgreSQL полностью запущен
            await using var cmd = new NpgsqlCommand("SELECT 1", connection);
            var result = await cmd.ExecuteScalarAsync();

            if (result?.ToString() != "1")
            {
                throw new InvalidOperationException("PostgreSQL is not fully initialized");
            }

            Log.Information("PostgreSQL is ready");
        });
    }

    public static async Task EnsureDatabaseCreatedAsync(IPollyService pollyService, IConfiguration configuration,
        bool createUser = false)
    {
        CommonValidator.CommonValidate(configuration);
        if (createUser)
        {
            CommonValidator.UserValidate(configuration);
        }

        // Создаем master connection string (к БД postgres)
        string masterConnectionString = ConnectionStringHelper.GetMasterConnectionString(configuration);

        // Создаем политику retry для подключения
        var retryPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount: 20,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(3),
                onRetry: (exception, _, retryAttempt, _) =>
                {
                    Log.Warning(
                        "Database connection retry {RetryAttempt}/20: {Message}",
                        retryAttempt, exception.Message);
                });

        await using var masterConnection = new NpgsqlConnection(masterConnectionString);

        await retryPolicy.ExecuteAsync(async () =>
        {
            await masterConnection.OpenAsync();
        });

        // Проверяем существование базы данных
        var databaseName = ConnectionStringHelper.GetDatabaseName(configuration);
        ArgumentException.ThrowIfNullOrEmpty(databaseName);
        var dbExists = await CheckDatabaseExistsAsync(masterConnection, databaseName);

        if (!dbExists)
        {
            // Создаем БД с ролью postgres
            await CreateDatabaseAsync(masterConnection, databaseName);

            if (createUser)
            {
                // Создаем пользователя приложения
                var (username, password) = ConnectionStringHelper.GetServiceUserInfo(configuration);
                ArgumentException.ThrowIfNullOrEmpty(username);
                ArgumentException.ThrowIfNullOrEmpty(password);
                // Подключаемся к созданной БД с ролью postgres
                await using var serviceMasterConnection = new NpgsqlConnection(ConnectionStringHelper.GetTargetMasterConnectionString(configuration));
                await serviceMasterConnection.OpenAsync();

                await CreateAppUserAsync(serviceMasterConnection, username, password);
            }
        }
    }

    public static async Task ApplyMigrationsAsync<TDbContext>(IServiceProvider serviceProvider) where TDbContext : DbContext
    {
        await ApplyMigrationsAsync<TDbContext>(serviceProvider, null);
    }

    public static async Task ApplyMigrationsAsync<TDbContext>(IServiceProvider serviceProvider, string? targetMigration) where TDbContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        // Получаем список всех миграций
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

        if (!pendingMigrations.Any())
        {
            return;
        }

        if (string.IsNullOrEmpty(targetMigration))
        {
            // Применяем все миграции
            await context.Database.MigrateAsync();
        }
        else
        {
            // Применяем миграции до указанной версии
            await context.Database.MigrateAsync(targetMigration);
        }
    }

    public static async Task<string> GetCurrentMigrationAsync<TDbContext>(IServiceProvider serviceProvider) where TDbContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var migrations = await context.Database.GetAppliedMigrationsAsync();
        return migrations.LastOrDefault() ?? "No migrations applied";
    }

    public static string GetTableName(Type type)
    {
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (tableAttr != null)
            return tableAttr.Name;

        return type.Name;
    }

    public static string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }

    #endregion Create&Migrations

    #region Private Methods

    private static async Task<bool> CheckDatabaseExistsAsync(NpgsqlConnection connection, string databaseName)
    {
        var commandText = @"
            SELECT 1 FROM pg_database 
            WHERE datname = @databaseName";

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@databaseName", databaseName);

        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    private static async Task CreateDatabaseAsync(NpgsqlConnection connection, string databaseName)
    {
        // Создаем БД с UTF8 кодировкой
        var commandText = $@"
            CREATE DATABASE {QuoteIdentifier(databaseName)}
            WITH ENCODING = 'UTF8'
            LC_COLLATE = 'en_US.utf8'
            LC_CTYPE = 'en_US.utf8'
            TEMPLATE = template0;";

        await using var command = new NpgsqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task CreateAppUserAsync(NpgsqlConnection serviceMasterConnection, string username, string password)
    {
        // Проверяем, существует ли пользователь
        var checkUserCommand = new NpgsqlCommand("SELECT 1 FROM pg_roles WHERE rolname = @username", serviceMasterConnection);
        checkUserCommand.Parameters.AddWithValue("@username", username);

        var userExists = await checkUserCommand.ExecuteScalarAsync() != null;

        var normalizedDatabaseName = QuoteIdentifier(serviceMasterConnection.Database);
        var normalizedUsername = QuoteIdentifier(username);
        var normalizedPassword = password.Replace("'", "''");

        if (!userExists)
        {
            // Создаем пользователя
            var createUserCommand = new NpgsqlCommand($"CREATE USER {normalizedUsername} WITH PASSWORD '{normalizedPassword}';", serviceMasterConnection);
            await createUserCommand.ExecuteNonQueryAsync();
        }

        // Даем права на базу данных
        var grantCommand = new NpgsqlCommand(
            $"GRANT ALL PRIVILEGES ON DATABASE {normalizedDatabaseName} TO {normalizedUsername}",
            serviceMasterConnection);
        await grantCommand.ExecuteNonQueryAsync();

        // Подключаемся к новой БД, чтобы выдать права на схемы и таблицы
        var schemaCommands = new[]
        {
            $"GRANT ALL PRIVILEGES ON DATABASE {normalizedDatabaseName} TO {normalizedUsername}",
            $"GRANT USAGE ON SCHEMA public TO {normalizedUsername}",
            $"GRANT CREATE ON SCHEMA public TO {normalizedUsername}",
            $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO {normalizedUsername}",
            $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO {normalizedUsername}",
            $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON FUNCTIONS TO {normalizedUsername}",
            $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TYPES TO {normalizedUsername}",
            $"GRANT ALL ON SCHEMA public TO {normalizedUsername}",
            $"GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO {normalizedUsername}",
            $"GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO {normalizedUsername}"
        };

        foreach (var cmdText in schemaCommands)
        {
            await using var cmd = new NpgsqlCommand(cmdText, serviceMasterConnection);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    #endregion
}