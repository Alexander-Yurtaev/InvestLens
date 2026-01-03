using Microsoft.Extensions.Configuration;

namespace InvestLens.Abstraction.Services;

public interface IDatabaseService
{
    Task EnsureDatabaseCreatedAsync(IConfiguration configuration);

    Task ApplyMigrationsAsync(IServiceProvider serviceProvider, string? targetMigration = null);
}