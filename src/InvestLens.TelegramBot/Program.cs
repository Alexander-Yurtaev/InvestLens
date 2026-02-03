using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using DotNetEnv.Configuration;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Redis.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Redis.Extensions;
using InvestLens.Shared.Redis.Services;
using InvestLens.Shared.Services;
using InvestLens.Shared.Validators;
using InvestLens.TelegramBot.Handlers;
using InvestLens.TelegramBot.Models;
using InvestLens.TelegramBot.Services;
using Serilog;
using Serilog.Events;
using ErrorEventHandler = InvestLens.TelegramBot.Handlers.ErrorEventHandler;

namespace InvestLens.TelegramBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration
            .AddDotNetEnv();

        builder.Services.AddDefaultCorrelationId(options =>
        {
            options.RequestHeader = HeaderConstants.CorrelationHeader;
            options.ResponseHeader = HeaderConstants.CorrelationHeader;
            options.IncludeInResponse = true;
            options.AddToLoggingScope = true; // Автоматически добавляет в LogContext
            options.LoggingScopeKey = "CorrelationId";
        });

        builder.Services.AddSingleton<ICorrelationIdService, CorrelationIdService>();

        // Конфигурация Serilog перед созданием хоста
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Services.Configure<TelegramSettings>(options =>
        {
            TelegramValidator.Validate(builder.Configuration);

            options.BaseAddress = builder.Configuration["Telegram:BaseAddress"]!;
            options.BotToken = builder.Configuration["BOT_TOKEN"]!;
            options.ChatId = builder.Configuration["CHAT_ID"]!;
        });

        // Добавляем Serilog в DI
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        try
        {
            builder.Services.AddSingleton<IPollyService, PollyService>();
            builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

            builder.Services.AddSingleton<IBotCommandService, BotCommandService>();
            builder.Services.AddHttpClient<ITelegramBotClient, TelegramBotClient>(client => { client.BaseAddress = new Uri("https://api.telegram.org/"); })
                .AddCorrelationIdForwarding()
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpRetryPolicy())
                .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpCircuitBreakerPolicy());

            // Redis
            builder.Services.AddRedisSettings(builder.Configuration).AddRedisClient();

            // RabbitMQ
            builder.Services.AddRabbitMqSettings(builder.Configuration).AddRabbitMqClient();
            builder.Services.AddScoped<InformationEventHandler>();
            builder.Services.AddScoped<ErrorEventHandler>();

            builder.Services.AddSingleton<IRefreshStatusService, RefreshStatusService>();
            builder.Services.AddHostedService<InvestLensBot>();

            var host = builder.Build();

            var messageBus = host.Services.GetRequiredService<IMessageBusClient>();
            await messageBus.SubscribeAsync<StartMessage, InformationEventHandler>(
                queueName: BusClientConstants.TelegramStartQueue,
                exchangeName: BusClientConstants.TelegramExchangeName,
                routingKey: BusClientConstants.TelegramStartKey);

            await messageBus.SubscribeAsync<CompleteMessage, InformationEventHandler>(
                queueName: BusClientConstants.TelegramCompleteQueue,
                exchangeName: BusClientConstants.TelegramExchangeName,
                routingKey: BusClientConstants.TelegramCompleteKey);

            await messageBus.SubscribeAsync<ErrorMessage, ErrorEventHandler>(
                queueName: BusClientConstants.TelegramErrorQueue,
                exchangeName: BusClientConstants.TelegramExchangeName,
                routingKey: BusClientConstants.TelegramErrorKey);

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Приложение остановлено из‑за исключения");
            throw;
        }
    }
}