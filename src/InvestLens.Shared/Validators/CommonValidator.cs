using InvestLens.Shared.MessageBus.Data;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Validators;

public static class CommonValidator
{
    public static void CommonValidate(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_HOST"], "DB_HOST");
        ArgumentException.ThrowIfNullOrEmpty(configuration["POSTGRES_PASSWORD"], "POSTGRES_PASSWORD");
    }

    public static void UserValidate(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_USER"], "DB_USER");
        ArgumentException.ThrowIfNullOrEmpty(configuration["DB_PASSWORD"], "DB_PASSWORD");
    }

    public static void MigrationValidate(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["TargetMigration"], "TargetMigration");
    }
}