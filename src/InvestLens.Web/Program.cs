using HealthChecks.UI.Client;
using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.Redis.Data;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Services;
using InvestLens.Shared.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace InvestLens.Web;

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
            builder.Services.AddRazorPages();

            RabbitMqValidator.Validate(builder.Configuration);
            builder.Services.AddRedisSettings(builder.Configuration);
            builder.Services.AddRabbitMqSettings(builder.Configuration);
            builder.Services.AddRabbitMqSettings(builder.Configuration);

            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            builder.Services.AddHealthChecks()
                .AddNpgSql(ConnectionStringHelper.GetTargetConnectionString(builder.Configuration),
                    name: "PostgreSQL", tags: ["database", "infrastructure"])
                .AddRedis(sp => sp.GetService<IRedisSettings>()?.ConnectionString ?? "",
                    name: "Redis", tags: ["cache", "infrastructure"])
                .AddRabbitMQ(async sp =>
                    {
                        var settings = sp.GetService<IRabbitMqSettings>()!;
                        var service = sp.GetService<IRabbitMqService>()!;
                        return await service.GetConnection(settings, CancellationToken.None);
                    },
                    name: "RabbitMQ", tags: ["queue", "infrastructure"]);

            // Добавляем Health Checks UI с хранилищем в PostgreSQL
            CommonValidator.CommonValidate(builder.Configuration);
            CommonValidator.UserValidate(builder.Configuration);
            await EnsureDatabaseInitAsync(builder.Configuration);
            builder.Services.AddHealthChecksUI(setup =>
                {
                    setup.SetHeaderText("InvestLens - System Health Dashboard");
                    setup.AddHealthCheckEndpoint("InvestLens System", "/health");
                    setup.AddHealthCheckEndpoint("Data API", "https://localhost:5011/health");
                    setup.AddHealthCheckEndpoint("Worker Service", "https://localhost:5021/health");

                    // Настройка интервала опроса
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.SetApiMaxActiveRequests(3);
                })
                .AddInMemoryStorage();
            //.AddPostgreSqlStorage(ConnectionStringHelper.GetTargetConnectionString(builder.Configuration));


            var app = builder.Build();

            // 3. Использование Serilog для логирования запросов
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Health Checks UI dashboard
            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
                options.ApiPath = "/health-ui-api";
                options.AddCustomStylesheet("wwwroot/css/healthchecks.css");

                // Настройка темы
                options.AsideMenuOpened = true;
                options.PageTitle = "InvestLens Health Dashboard";
            });

            app.MapStaticAssets();
            app.MapRazorPages()
                .WithStaticAssets();

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

    private static async Task EnsureDatabaseInitAsync(IConfiguration configuration)
    {
        try
        {
            await DatabaseHelper.EnsureDatabaseCreatedAsync(configuration, true);
            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization fatal");
            throw;
        }
    }
}