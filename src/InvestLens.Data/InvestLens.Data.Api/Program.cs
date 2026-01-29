using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using HealthChecks.UI.Client;
using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Extensions;
using InvestLens.Data.Api.Handlers;
using InvestLens.Data.Api.Models.Settings;
using InvestLens.Data.Api.Services;
using InvestLens.Data.DataContext;
using InvestLens.Data.Repositories;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Redis.Services;
using InvestLens.Shared.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;

namespace InvestLens.Data.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDefaultCorrelationId(options =>
        {
            options.RequestHeader = HeaderConstants.CorrelationHeader;
            options.ResponseHeader = HeaderConstants.CorrelationHeader;
            options.IncludeInResponse = true;
            options.AddToLoggingScope = true; // Автоматически добавляет в LogContext
            options.LoggingScopeKey = "CorrelationId";
        });

        builder.Services.AddSingleton<ICorrelationIdService, CorrelationIdService>();

        // 1. Настройка Serilog
        Log.Logger = SerilogHelper.CreateLogger(builder);

        // 2. Добавление Serilog в DI
        builder.Host.UseSerilog((_, configuration) =>
        {
            configuration
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        });

        try
        {
            // Common
            var commonSettings = builder.Services.AddCommonSettings(builder.Configuration);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddGrpc();

            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            builder.Services
                .AddHttpClient<IMoexClient, MoexClient>(client => client.BaseAddress = new Uri(commonSettings.MoexBaseUrl))
                .AddCorrelationIdForwarding()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                })
                .AddPolicyHandler((provider, _) => provider.GetRequiredService<IPollyService>().GetHttpResilientPolicy());

            builder.Services.AddInvestLensDatabaseInfrastructure(builder.Configuration);
            builder.Services.AddScoped<ISecurityRepository, SecurityRepository>();
            builder.Services.AddScoped<IRefreshStatusRepository, RefreshStatusRepository>();
            builder.Services.AddScoped<IDataService, DataService>();

            builder.Services.AddSingleton<ISecuritiesRefreshStatusService, SecuritiesRefreshStatusService>();

            // Redis
            builder.Services.AddRedisSettings(builder.Configuration).AddRedisClient();

            // RabbitMQ
            builder.Services.AddRabbitMqSettings(builder.Configuration).AddRabbitMqClient();
            builder.Services.AddScoped<SecurityRefreshEventHandler>();

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

            app.UseCorrelationId();

            app.Use(async (context, next) =>
            {
                var correlationContextAccessor = context.RequestServices
                    .GetRequiredService<ICorrelationContextAccessor>();

                var correlationId = correlationContextAccessor.CorrelationContext?.CorrelationId;

                if (!string.IsNullOrEmpty(correlationId))
                {
                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        await next(context);
                    }
                }
                else
                {
                    await next(context);
                }
            });

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            await EnsureDatabaseInitAsync(app);

            using (var scope = app.Services.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetService<IRabbitMqService>()!;
                await rabbitMqService.EnsureRabbitMqIsRunningAsync(app.Configuration, CancellationToken.None);
            }

            var messageBus = app.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<SecurityRefreshMessage, SecurityRefreshEventHandler>(
                queueName: BusClientConstants.SecretesRefreshQueue,
                exchangeName: BusClientConstants.SecuritiesExchangeName,
                routingKey: BusClientConstants.DataSecuritiesRefreshKey);

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapGet("/", () => "Data Service");

            app.MapGet("/securities", (IDataService dataService) => dataService.GetSecurities(1, 10));

            app.MapGrpcService<SecurityGrpcService>();

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
        try
        {
            using var scope = app.Services.CreateScope();

            // 1. Сначала ждем доступность PostgreSQL
            await DatabaseHelper.WaitForPostgresAsync(app.Configuration);

            // 2. Создаем БД и пользователя
            var pollyService = scope.ServiceProvider.GetService<IPollyService>()!;
            await DatabaseHelper.EnsureDatabaseCreatedAsync(pollyService, app.Configuration, true);

            // 3. Получаем целевую миграцию
            var commonSettings = app.Services.GetService<ICommonSettings>();
            if (commonSettings is null)
            {
                throw new ArgumentException("CommonSettings must be initialized.");
            }

            // 4. Применяем миграции
            var targetMigration = commonSettings.TargetMigration;
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