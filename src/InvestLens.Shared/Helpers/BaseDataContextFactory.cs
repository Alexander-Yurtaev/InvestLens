using DotNetEnv.Configuration;
using InvestLens.Shared.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Helpers;

public abstract class BaseDataContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
{
    protected abstract string LocalEnvPath { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T CreateDbContext(string[] args)
    {
        Console.WriteLine("BaseDataContextFactory ...");

        Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Directory of the executable file: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine($"The path to the shared .env: {Path.GetFullPath(@"..\..\..\.env")}");
        Console.WriteLine($"The path to the local .env: {Path.GetFullPath(@"..\InvestLens.Data.Api\.env")}");

        Console.WriteLine("1. Download .env");
        DotNetEnv.Env.Load();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()  // Берём из переменных ОС (включая .env после Env.Load())
            .AddDotNetEnv(Path.GetFullPath(@"..\..\..\.env"))
            .AddDotNetEnv(Path.GetFullPath(LocalEnvPath))
            .Build();

        configuration["DB_HOST"] = "postgres_multi_db";

        CommonValidator.CommonValidate(configuration);
        CommonValidator.UserValidate(configuration);
        
        Console.WriteLine("2. Get connection string");
        var connectionString = ConnectionStringHelper.GetTargetLocalhostConnectionString(configuration);
        Console.WriteLine($"connectionString: {connectionString}");
        Console.WriteLine("3. Setting up context options");
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(connectionString);

        T instance = (T?)Activator.CreateInstance(typeof(T), optionsBuilder.Options)
                     ?? throw new InvalidOperationException($"Failed to create an instance of the type {typeof(T)}");

        return instance;
    }
}