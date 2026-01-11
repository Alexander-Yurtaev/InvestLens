using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Extensions;
using InvestLens.Data.Api.Handlers;
using InvestLens.Data.Api.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Repositories;
using InvestLens.Shared.Data;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Services;
using Serilog;

namespace InvestLens.Data.Api;

public static class Program
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
            ValidateSettings(builder.Configuration);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddScoped<IPollyService, PollyService>();

            string moexBaseUrl = builder.Configuration["MoexBaseUrl"]!;
            builder.Services
                .AddHttpClient("MoexClient", options => options.BaseAddress = new Uri(moexBaseUrl))
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetRetryPolicy())
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetCircuitBreakerPolicy());

            builder.Services.AddInvestLensDatabaseInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IMoexClient, MoexClient>();
            builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
            builder.Services.AddScoped<IRefreshStatusRepository, RefreshStatusRepository>();
            builder.Services.AddScoped<IDataService, DataService>();

            // Redis
            builder.Services.AddRedisClient(builder.Configuration);

            // RabbitMQ
            builder.Services.AddRabbitMqClient(builder.Configuration);
            builder.Services.AddScoped<SecurityRefreshingEventHandler>();

            var app = builder.Build();

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            await EnsureDatabaseInitAsync(app);

            var messageBus = app.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<SecurityRefreshingMessage, SecurityRefreshingEventHandler>(
                queueName: "securities-refresh-queue",
                exchangeName: BusClientConstants.ExchangeName,
                routingKey: BusClientConstants.SecuritiesRefreshingKey);

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

    private static async Task EnsureDatabaseInitAsync(WebApplication app)
    {
        ConnectionStringHelper.ValidateMigrationConfigurations(app.Configuration);

        try
        {
            using var scope = app.Services.CreateScope();

            // 2.1 Создаем БД и пользователя
            await DatabaseHelper.EnsureDatabaseCreatedAsync(app.Configuration, true);

            // 2.2 Получаем целевую миграцию
            var targetMigration = ConnectionStringHelper.GetTargetMigration(app.Configuration);

            // 2.3 Применяем миграции
            if (string.IsNullOrEmpty(targetMigration))
            {
                await DatabaseHelper.ApplyMigrationsAsync<InvestLensDataContext>(scope.ServiceProvider);
            }
            else
            {
                await DatabaseHelper.ApplyMigrationsAsync<InvestLensDataContext>(scope.ServiceProvider, targetMigration);
            }

            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization fatal");
            throw;
        }
    }

    private static void ValidateSettings(IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["MoexBaseUrl"], "MoexBaseUrl");
    }
}