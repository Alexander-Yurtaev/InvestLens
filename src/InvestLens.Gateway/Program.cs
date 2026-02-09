using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using InvestLens.Gateway.Extensions;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Services;
using Serilog;
using Serilog.Context;

namespace InvestLens.Gateway;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
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

        builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

        // Добавляем YARP
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

        app.UseHttpsRedirection();

        app.AddSecurities(builder.Configuration["DATA_BASE_ADDRESS"]);
        app.AddDictionaries(builder.Configuration["DATA_BASE_ADDRESS"]);

        // Проксируем остальные запросы через YARP
        app.MapReverseProxy();

        await app.RunAsync();
    }
}