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

        Console.WriteLine("1. Загружаем .env (если используете)");
        var env = DotNetEnv.Env.Load();
        EnvWriteLine(env);

        Console.WriteLine("2. Получаем строку подключения");
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()  // Берём из переменных ОС (включая .env после Env.Load())
            .Build();

        var connectionString = ConnectionStringHelper.GetTargetConnectionString(configuration);
        Console.WriteLine($"ConnectionString: {connectionString}");

        Console.WriteLine("3. Настраиваем опции контекста");
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        optionsBuilder.UseNpgsql(connectionString);

        T instance = (T?)Activator.CreateInstance(typeof(T), optionsBuilder.Options)
                     ?? throw new InvalidOperationException($"Не удалось создать экземпляр типа {typeof(T)}");

        return instance!;
    }

    private static void EnvWriteLine(IEnumerable<KeyValuePair<string, string>> env)
    {
        Console.WriteLine("=====Start - EnvWriteLine=====");
        if (!env.Any())
        {
            Console.WriteLine("env is empty");
        }
        else
        {
            foreach (var pair in env)
            {
                Console.WriteLine($"{pair.Key} = {pair.Value}");
            }
        }
        Console.WriteLine("=====Finish - EnvWriteLine=====");
    }
}