using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Extensions;
using InvestLens.Data.Api.Services;
using InvestLens.Data.Repositories;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Services;
using Serilog;

namespace InvestLens.Data.Api;

public static partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Настройка Serilog
        Log.Logger = SerilogHelper.CreateLogger(builder);

        // 2. Добавление Serilog в DI
        builder.Host.UseSerilog();

        try
        {
            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            string? moexBaseUrl = builder.Configuration["MOEX_BASE_URL"];
            ArgumentException.ThrowIfNullOrEmpty(moexBaseUrl, "MOEX_BASE_URL");
            builder.Services.AddHttpClient("MoexClient", options => options.BaseAddress = new Uri(moexBaseUrl));

            builder.Services.AddInvestLensDatabaseInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IMoexClient, MoexClient>();
            builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
            builder.Services.AddScoped<IRefreshStatusRepository, RefreshStatusRepository>();
            builder.Services.AddScoped<IDataService, DataService>();

            var app = builder.Build();

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

            ValidateSettings(builder.Configuration);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            await app.EnsureDatabaseInitAsync();

            app.MapGet("/", () =>
                Results.Content(
                    "<html><body>" +
                    "<a href='securities'>securities</a>" +
                    "</body></html>",
                    "text/html"
                ));
            app.MapGet("/securities", (IDataService dataService) => dataService.GetSecurities());

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Приложение остановлено из‑за исключения");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task EnsureDatabaseInitAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

            // Получаем параметры из конфигурации
            var targetMigration = PostgresDataHelper.GetTargetMigration(app.Configuration);

            // 2.1 Создаем БД и пользователя
            await databaseService.EnsureDatabaseCreatedAsync(app.Configuration);

            // 2.2 Применяем миграции
            if (string.IsNullOrEmpty(targetMigration))
            {
                await databaseService.ApplyMigrationsAsync(scope.ServiceProvider);
            }
            else
            {
                await databaseService.ApplyMigrationsAsync(scope.ServiceProvider, targetMigration);
            }

            Log.Information("✅ Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "⚠️ Database initialization fatal");
            throw;
        }
    }
}