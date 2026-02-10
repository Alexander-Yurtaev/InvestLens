using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using InvestLens.Gateway.Extensions;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Services;
using Microsoft.OpenApi.Models;
using Prometheus;
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "InvestLens Gateway API",
                Version = "v1",
                Description = "Gateway API for InvestLens application with YARP reverse proxy"
            });

            options.TagActionsBy(api =>
            {
                var tag = api.RelativePath?.Split('/').FirstOrDefault(s => !string.IsNullOrEmpty(s));
                return tag is not null ? [tag] : ["Default"];
            });

            // Опционально: добавить комментарии XML
            //var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //if (File.Exists(xmlPath))
            //{
            //    options.IncludeXmlComments(xmlPath);
            //}
        });

        builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

        // Добавляем YARP
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestLens Gateway API v1");
                options.RoutePrefix = "swagger"; // Доступ по /swagger
                options.DocumentTitle = "InvestLens Gateway API Documentation";
                options.DisplayRequestDuration();
            });
        }

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

        app.UseHttpMetrics();
        app.MapMetrics();

        // Проксируем остальные запросы через YARP
        app.MapReverseProxy();

        await app.RunAsync();
    }
}