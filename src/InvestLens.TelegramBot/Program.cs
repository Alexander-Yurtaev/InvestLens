using DotNetEnv.Configuration;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Extensions;
using InvestLens.Shared.MessageBus.Models;
using InvestLens.Shared.Services;
using InvestLens.TelegramBot.Handlers;
using InvestLens.TelegramBot.Models;
using InvestLens.TelegramBot.Services;
using InvestLens.Shared.Validators;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddDotNetEnv();

builder.Services.Configure<TelegramSettings>(options =>
{
    TelegramValidator.Validate(builder.Configuration);

    options.BaseAddress = builder.Configuration["Telegram:BaseAddress"]!;
    options.BotToken = builder.Configuration["BOT_TOKEN"]!;
    options.ChatId = builder.Configuration["CHAT_ID"]!;
});

builder.Services.AddSingleton<IPollyService, PollyService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.telegram.org/");
    })
    .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpRetryPolicy())
    .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpCircuitBreakerPolicy());

// RabbitMQ
builder.Services.AddRabbitMqSettings(builder.Configuration).AddRabbitMqClient();
builder.Services.AddScoped<InformationEventHandler>();
builder.Services.AddScoped<InvestLens.TelegramBot.Handlers.ErrorEventHandler>();

//builder.Services.AddHostedService<Worker>();

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

await messageBus.SubscribeAsync<ErrorMessage, InvestLens.TelegramBot.Handlers.ErrorEventHandler>(
    queueName: BusClientConstants.TelegramErrorQueue,
    exchangeName: BusClientConstants.TelegramExchangeName,
    routingKey: BusClientConstants.TelegramErrorKey);

host.Run();
