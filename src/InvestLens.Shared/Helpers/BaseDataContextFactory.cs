using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Helpers;

public abstract class BaseDataContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T CreateDbContext(string[] args)
    {
        Console.WriteLine("BaseDataContextFactory ...");

        Console.WriteLine($"Текущая директория: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Директория исполняемого файла: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine($"Путь к .env: {Path.GetFullPath(@"..\..\.env")}");

        Console.WriteLine("1. Загружаем .env");
        DotNetEnv.Env.Load();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()  // Берём из переменных ОС (включая .env после Env.Load())
            .Build();

        Console.WriteLine("2. Получаем строку подключения");
        var connectionString = ConnectionStringHelper.GetTargetConnectionString(configuration);

        Console.WriteLine("3. Настраиваем опции контекста");
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(connectionString);

        T instance = (T?)Activator.CreateInstance(typeof(T), optionsBuilder.Options)
                     ?? throw new InvalidOperationException($"Не удалось создать экземпляр типа {typeof(T)}");

        return instance;
    }
}