using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using HealthChecks.UI.Client;
using InvestLens.Gateway.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Extensions;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Services;
using InvestLens.Shared.Services.RabbitMq;
using InvestLens.Shared.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using System.Net;

namespace InvestLens.Web;

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
            builder.Services.AddRazorPages();

            RabbitMqValidator.Validate(builder.Configuration);
            builder.Services.AddRedisSettings(builder.Configuration);
            builder.Services.AddRabbitMqSettings(builder.Configuration);

            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

            builder.Services.AddScoped<ISecurityGrpcClient, SecurityGrpcClient>();
            builder.Services.AddScoped<IEngineDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<IMarketDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<IBoardDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<IBoardGroupDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<IDurationDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<ISecurityTypeDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<ISecurityGroupDictionariesGrpcClient, GlobalDictionariesGrpcClient>();
            builder.Services.AddScoped<ISecurityCollectionDictionariesGrpcClient, GlobalDictionariesGrpcClient>();

            builder.Services.AddHealthChecks()
                .AddUrlGroup(new Uri("https://investlens.worker:8081/health"),
                    httpMethod: HttpMethod.Get,
                    name: "Worker Service", tags: ["worker", "job"])
                .AddUrlGroup(new Uri("https://investlens.data.api:8081/health"),
                    httpMethod: HttpMethod.Get,
                    name: "Data API", tags: ["data", "api"]);

            CommonValidator.CommonValidate(builder.Configuration);
            CommonValidator.UserValidate(builder.Configuration);

            // Добавляем Health Checks UI с хранилищем в PostgreSQL
            builder.Services.AddHealthChecksUI(setup =>
                {
                    setup.SetHeaderText("InvestLens - System Health Dashboard");
                    setup.AddHealthCheckEndpoint("InvestLens System", "http://localhost:8080/health");
                    setup.AddHealthCheckEndpoint("Data API", "https://investlens.data.api:8081/health");
                    setup.AddHealthCheckEndpoint("Worker Service", "https://investlens.worker:8081/health");

                    // Настройка интервала опроса
                    setup.SetEvaluationTimeInSeconds(30);
                    setup.SetApiMaxActiveRequests(3);

                    setup.DisableDatabaseMigrations();
                })
                .AddInMemoryStorage();

            builder.Services.AddHttpClient("DataApiClient", client =>
                {
                    var baseAddress = builder.Configuration["GatewayAddress"];
                    if (string.IsNullOrEmpty(baseAddress)) throw new InvalidOperationException("Сервис не настроен.");
                    client.BaseAddress = new Uri(baseAddress);
                })
                .AddCorrelationIdForwarding()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    // Используем HTTP/2 при поддержке сервером
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.All,

                    // Для HTTP/2 с TLS (HTTPS)
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
                })
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpRetryPolicy())
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpCircuitBreakerPolicy());

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

            using var scope = app.Services.CreateScope();
            var pollyService = scope.ServiceProvider.GetService<IPollyService>()!;
            await EnsureDatabaseInitAsync(pollyService, app.Configuration);

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

            // Для исключения health checks из HTTPS редиректа
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"), appBuilder =>
            {
                appBuilder.UseHttpsRedirection();
            });

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

    private static async Task EnsureDatabaseInitAsync(IPollyService pollyService, IConfiguration configuration)
    {
        try
        {
            await DatabaseHelper.EnsureDatabaseCreatedAsync(pollyService, configuration, true);
            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database initialization fatal");
            throw;
        }
    }
}