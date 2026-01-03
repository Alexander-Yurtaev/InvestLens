using InvestLens.Abstraction.Services;
using InvestLens.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace InvestLens.Shared.Services;

public class DatabaseService<TContext> : IDatabaseService where TContext : DbContext

{
    private readonly ILogger<DatabaseService<TContext>> _logger;

    public DatabaseService(ILogger<DatabaseService<TContext>> logger)
    {
        _logger = logger;
    }


    public async Task EnsureDatabaseCreatedAsync(IConfiguration configuration)
    {
        try
        {
            // Создаем master connection string (к БД postgres)
            string masterConnectionString = PostgresDataHelper.GetMasterConnectionString(configuration);
            await using var masterConnection = new NpgsqlConnection(masterConnectionString);
            await masterConnection.OpenAsync();

            // Проверяем существование базы данных
            var databaseName = PostgresDataHelper.GetDatabaseName(configuration);
            ArgumentException.ThrowIfNullOrEmpty(databaseName);
            var dbExists = await CheckDatabaseExistsAsync(masterConnection, databaseName);

            if (!dbExists)
            {
                _logger.LogInformation("Database '{DatabaseName}' does not exist. Creating...", databaseName);

                // Создаем БД с ролью postgres
                await CreateDatabaseAsync(masterConnection, databaseName);
                _logger.LogInformation("Database '{DatabaseName}' created successfully", databaseName);

                // Создаем пользователя приложения
                var (username, password) = PostgresDataHelper.GetServiceUserInfo(configuration);
                ArgumentException.ThrowIfNullOrEmpty(username);
                ArgumentException.ThrowIfNullOrEmpty(password);
                // Подключаемся к созданной БД с ролью postgres
                await using var serviceMasterConnection = new NpgsqlConnection(PostgresDataHelper.GetTargetMasterConnectionString(configuration));
                await serviceMasterConnection.OpenAsync();

                await CreateAppUserAsync(serviceMasterConnection, username, password);
                _logger.LogInformation("Application user '{User}' created for database '{Database}'",
                    username, databaseName);
            }
            else
            {
                _logger.LogInformation("Database '{Database}' already exists", databaseName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring database creation");
            throw;
        }

    }

    public async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        await ApplyMigrationsAsync(serviceProvider, null);
    }

    public async Task ApplyMigrationsAsync(
        IServiceProvider serviceProvider,
        string? targetMigration)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            _logger.LogInformation("Checking for pending migrations...");

            // Получаем список всех миграций
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (!pendingMigrations.Any())
            {
                _logger.LogInformation("No pending migrations found");
                return;
            }

            _logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                pendingMigrations.Count, string.Join(", ", pendingMigrations));

            if (string.IsNullOrEmpty(targetMigration))
            {
                // Применяем все миграции
                await context.Database.MigrateAsync();
                _logger.LogInformation("All migrations applied successfully");
            }
            else
            {
                // Применяем миграции до указанной версии
                await context.Database.MigrateAsync(targetMigration);
                _logger.LogInformation("Migrations applied up to '{TargetMigration}'", targetMigration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying migrations");
            throw;
        }
    }

    public async Task<string> GetCurrentMigrationAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            var migrations = await context.Database.GetAppliedMigrationsAsync();
            return migrations.LastOrDefault() ?? "No migrations applied";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current migration");
            return "Error";
        }
    }

    #region Private Methods

    private async Task<bool> CheckDatabaseExistsAsync(NpgsqlConnection connection, string databaseName)
    {
        var commandText = @"
            SELECT 1 FROM pg_database 
            WHERE datname = @databaseName";

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@databaseName", databaseName);

        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    private async Task CreateDatabaseAsync(NpgsqlConnection connection, string databaseName)
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

    private async Task CreateAppUserAsync(NpgsqlConnection serviceMasterConnection, string username, string password)
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

    private string QuoteIdentifier(string identifier)
    {
        // Экранируем идентификаторы для защиты от SQL injection
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }

    #endregion

}