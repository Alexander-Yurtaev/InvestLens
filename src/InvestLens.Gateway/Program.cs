using AutoMapper;
using CorrelationId;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using Grpc.Net.Client;
using InvestLens.Grpc.Service;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Models;
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

        app.MapGet("/api/data/securities", async (
            IMapper mapper,
            int page,
            int pageSize,
            string? sort="",
            string? filter=""
            ) =>
        {
            try
            {
                // Создаем gRPC клиент
                var dataBaseAddress = builder.Configuration["DATA_BASE_ADDRESS"];
                if (string.IsNullOrEmpty(dataBaseAddress)) throw new InvalidOperationException("Ошибка в настроках.");

                using var channel = GrpcChannel.ForAddress(dataBaseAddress);
                var client = new SecurityServices.SecurityServicesClient(channel);

                // Вызываем gRPC метод
                var response = await client.GetSecuritiesWithDetailsAsync(new GetPaginationRequest()
                    { Page = page, PageSize = pageSize, Sort = sort, Filter = filter });

                // Преобразуем gRPC ответ в REST формат
                var securities = mapper.Map<SecurityWithDetailsModelWithPagination>(response);

                return Results.Ok(securities);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error calling data service: {ex.Message}");
            }
        });

        // Проксируем остальные запросы через YARP
        app.MapReverseProxy();

        await app.RunAsync();
    }
}