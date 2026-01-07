using Hangfire;
using Hangfire.PostgreSql;
using InvestLens.Shared.Filters;
using InvestLens.Shared.Helpers;
using Serilog;

namespace InvestLens.Worker;

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
            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Добавляем Hangfire
            ConnectionStringHelper.ValidateCommonConfigurations(builder.Configuration);
            ConnectionStringHelper.ValidateUserConfigurations(builder.Configuration);

            var hangfireConnectionString = ConnectionStringHelper.GetTargetConnectionString(builder.Configuration);
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(hangfireConnectionString);
                });
            });

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2; // Настройте по необходимости
                options.ServerName = "Hangfire-Microservice";
            });

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

            builder.Services.AddHealthChecks()
                .AddHangfire(options =>
                {
                    options.MinimumAvailableServers = 1;
                });

            var app = builder.Build();

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors("AllowAll");

            await EnsureDatabaseInitAsync(app);

            app.MapHealthChecks("/health");

            // Настройка dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization =
                [
                    new HangfireDashboardAuthorizationFilter()
                ],
                AppPath = "/hangfire", // URL возврата
                IgnoreAntiforgeryToken = true
            });

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
            Log.CloseAndFlush();
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