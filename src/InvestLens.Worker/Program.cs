using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.States;
using HealthChecks.UI.Client;
using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Filters;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Services;
using InvestLens.Shared.Validators;
using InvestLens.Worker.Extensions;
using InvestLens.Worker.Filters;
using InvestLens.Worker.Handlers;
using InvestLens.Worker.Jobs;
using InvestLens.Worker.Models;
using InvestLens.Worker.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;

namespace InvestLens.Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Загружаем конфигурацию из нескольких файлов
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Jobs.json", optional: false, reloadOnChange: true) // ← наш файл!
            .AddEnvironmentVariables();

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
            // Add services to the container.
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IPollyService, PollyService>();

            // Добавляем Hangfire
            CommonValidator.CommonValidate(builder.Configuration);
            CommonValidator.UserValidate(builder.Configuration);

            // Job
            builder.Services.AddJobSettings(builder.Configuration);

            #region Registration of services

            // 1. Регистрация сервисов
            builder.Services.AddScoped<ISecuritiesService, SecuritiesService>();

            // 2. Регистрация конфигурируемого планировщика
            builder.Services.Configure<HangfireJobsConfiguration>(builder.Configuration.GetSection("HangfireJobs"));
            builder.Services.AddSingleton<IConfigurableJobScheduler, ConfigurableJobScheduler>();

            builder.Services.AddSingleton<IElectStateFilter, NoRetryForSpecificExceptionsFilter>();

            #endregion Registration of services

            #region AddHangfire

            var hangfireConnectionString = ConnectionStringHelper.GetTargetConnectionString(builder.Configuration);
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(hangfireConnectionString);
                },
                new PostgreSqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15), // Как часто проверять очередь
                    DistributedLockTimeout = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true // Автоматически создает таблицы
                });
            });

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2; // Оптимальное количество воркеров
                options.Queues = ["default", "critical", "daily"]; // Разные очереди
                // options.ServerName = $"Worker-{Environment.MachineName}";
                options.ServerName = "Hangfire-Microservice";
            });

            builder.Services.AddHealthChecks()
                .AddHangfire(options =>
                {
                    options.MinimumAvailableServers = 1;
                });

            #endregion AddHangfire

            // Настройка CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // Redis
            builder.Services.AddRedisSettings(builder.Configuration).AddRedisClient();

            // RabbitMQ
            builder.Services.AddRabbitMqSettings(builder.Configuration).AddRabbitMqClient();
            builder.Services.AddScoped<SecurityRefreshEventHandler>();

            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            builder.Services.AddHealthChecks()
                .AddNpgSql(ConnectionStringHelper.GetTargetConnectionString(builder.Configuration))
                .AddRedis(sp => sp.GetService<IRedisSettings>()?.ConnectionString ?? "")
                .AddRabbitMQ(async sp =>
                {
                    var settings = sp.GetService<IRabbitMqSettings>()!;
                    var service = sp.GetService<IRabbitMqService>()!;
                    return await service.GetConnection(settings, CancellationToken.None);
                });

            builder.Services.AddHealthChecksUI(setup =>
                {
                    setup.SetHeaderText("Worker Service Health");
                    setup.SetEvaluationTimeInSeconds(30);

                    setup.DisableDatabaseMigrations();
                })
                .AddInMemoryStorage();

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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            using (var scope = app.Services.CreateScope())
            {
                var filter = scope.ServiceProvider.GetService<IElectStateFilter>();
                GlobalJobFilters.Filters.Add(filter);
            }

            app.UseCors("AllowAll");

            await DatabaseHelper.EnsureDatabaseInitAsync(app);

            using (var scope = app.Services.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetService<IRabbitMqService>()!;
                await rabbitMqService.EnsureRabbitMqIsRunningAsync(app.Configuration, CancellationToken.None);
            }

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.PageTitle = "Worker Service Health";
            });

            var messageBus = app.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<SecurityRefreshMessage, SecurityRefreshEventHandler>(
                queueName: BusClientConstants.SecretesRefreshQueue,
                exchangeName: BusClientConstants.SecuritiesExchangeName,
                routingKey: BusClientConstants.WorkerSecuritiesRefreshKey);

            #region UseHangfire

            app.MapHangfireDashboard("/metrics", new DashboardOptions
            {
                DashboardTitle = "Metrics",
                AppPath = null,
                DisplayStorageConnectionString = false,
                IgnoreAntiforgeryToken = true
            });

            // 5. Настройка Dashboard
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                DashboardTitle = "Worker Jobs Dashboard",
                Authorization = [new HangfireDashboardAuthorizationFilter()],
                DarkModeEnabled = true,
                StatsPollingInterval = 5000, // Обновление статистики каждые 5 сек
                AppPath = "/jobs", // URL возврата
                IgnoreAntiforgeryToken = true
            });

            // 6. API для управления задачами
            app.MapGet("/api/jobs", (IConfigurableJobScheduler scheduler) =>
            {
                return scheduler.GetRegisteredJobs();
            });

            app.MapPost("/api/jobs/{jobId}/reschedule", (string jobId, string cron, IConfigurableJobScheduler scheduler) =>
            {
                scheduler.UpdateJobSchedule(jobId, cron);
                return Results.Ok(new { Message = $"Расписание задачи {jobId} обновлено" });
            });

            app.MapPost("/api/jobs/reload", (IConfigurableJobScheduler scheduler) =>
            {
                // Удаляем все существующие задачи
                RecurringJob.RemoveIfExists("");

                // Регистрируем заново из конфигурации
                scheduler.ScheduleRecurringJobs();

                return Results.Ok(new { Message = "Все задачи перезагружены из конфигурации" });
            });

            // 7. Запуск планировщика при старте приложения
            using (var scope = app.Services.CreateScope())
            {
                var scheduler = scope.ServiceProvider.GetRequiredService<IConfigurableJobScheduler>();

                // Регистрируем повторяющиеся задачи
                scheduler.ScheduleRecurringJobs();

                // Планируем задачи при старте
                scheduler.ScheduleStartupJobs();
            }

            #endregion UseHangfire

            app.MapGet("/", () => "Worker service");

            app.UseHttpsRedirection();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Приложение остановлено из‑за исключения");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}