using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.States;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Filters;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Worker.Filters;
using InvestLens.Worker.Handlers;
using InvestLens.Worker.Jobs;
using InvestLens.Worker.Models;
using InvestLens.Worker.Services;
using Serilog;

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

        // 1. Настройка Serilog
        Log.Logger = SerilogHelper.CreateLogger(builder);

        // 2. Добавление Serilog в DI
        builder.Host.UseSerilog();

        try
        {
            // Add services to the container.
            builder.Services.AddOpenApi();

            // Добавляем Hangfire
            ConnectionStringHelper.ValidateCommonConfigurations(builder.Configuration);
            ConnectionStringHelper.ValidateUserConfigurations(builder.Configuration);

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
                new Hangfire.PostgreSql.PostgreSqlStorageOptions
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
            builder.Services.AddRedisClient(builder.Configuration);

            // RabbitMQ
            builder.Services.AddRabbitMqClient(builder.Configuration);
            builder.Services.AddScoped<SecurityRefreshEventHandler>();

            var app = builder.Build();

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

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

            await EnsureDatabaseInitAsync(app);

            app.MapHealthChecks("/health");

            var messageBus = app.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<SecurityRefreshMessage, SecurityRefreshEventHandler>(
                queueName: "securities-refresh-queue",
                exchangeName: BusClientConstants.ExchangeName,
                routingKey: BusClientConstants.SecuritiesRefreshKey);

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

    private static async Task EnsureDatabaseInitAsync(WebApplication app)
    {
        ConnectionStringHelper.ValidateCommonConfigurations(app.Configuration);

        try
        {
            using var scope = app.Services.CreateScope();

            // Создаем БД
            await DatabaseHelper.EnsureDatabaseCreatedAsync(app.Configuration, true);

            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization fatal");
            throw;
        }
    }
}