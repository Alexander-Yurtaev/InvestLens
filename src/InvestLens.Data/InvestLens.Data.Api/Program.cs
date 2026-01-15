using HealthChecks.UI.Client;
using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Extensions;
using InvestLens.Data.Api.Handlers;
using InvestLens.Data.Api.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Repositories;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Services;
using InvestLens.Shared.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
            MoexValidator.Validate(builder.Configuration);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            string moexBaseUrl = builder.Configuration["MoexBaseUrl"]!;
            builder.Services
                .AddHttpClient("MoexClient", options => options.BaseAddress = new Uri(moexBaseUrl))
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpRetryPolicy())
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpCircuitBreakerPolicy());

            builder.Services.AddInvestLensDatabaseInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IMoexClient, MoexClient>();
            builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
            builder.Services.AddScoped<IRefreshStatusRepository, RefreshStatusRepository>();
            builder.Services.AddScoped<IDataService, DataService>();

            // Redis
            builder.Services.AddRedisSettings(builder.Configuration).AddRedisClient();

            // RabbitMQ
            builder.Services.AddRabbitMqSettings(builder.Configuration).AddRabbitMqClient();
            builder.Services.AddScoped<SecurityRefreshingEventHandler>();

            builder.Services.AddHealthChecks()
                .AddNpgSql(ConnectionStringHelper.GetTargetConnectionString(builder.Configuration))
                .AddRedis(sp => sp.GetService<IRedisSettings>()?.ConnectionString ?? "")
                .AddRabbitMQ(async sp =>
                {
                    var settings = sp.GetService<IRabbitMqSettings>()!;
                    var service = sp.GetService<IRabbitMqService>()!;
                    return await service.GetConnection(settings, CancellationToken.None);
                });

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

            var rabbitMqService = app.Services.GetService<IRabbitMqService>()!;
            await rabbitMqService.EnsureRabbitMqIsRunningAsync(app.Configuration, CancellationToken.None);

            var messageBus = app.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<SecurityRefreshingMessage, SecurityRefreshingEventHandler>(
                queueName: "securities-refresh-queue",
                exchangeName: BusClientConstants.ExchangeName,
                routingKey: BusClientConstants.SecuritiesRefreshingKey);

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

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
        CommonValidator.MigrationValidate(app.Configuration);

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
}