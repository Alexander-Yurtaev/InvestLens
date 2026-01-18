using System.Data;
using DotNetEnv;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Services;
using InvestLens.TelegramBot;
using InvestLens.TelegramBot.Models;
using InvestLens.TelegramBot.Services;

var builder = Host.CreateApplicationBuilder(args);

var env= Env.Load().ToDictionary(k => k.Key, v => v.Value);

builder.Services.Configure<TelegramSettings>(options =>
{
    var telegramSettings = builder.Configuration.GetSection("Telegram");
    options.BaseAddress = telegramSettings["BaseAddress"] ?? throw new DataException("Settings must be initialized.");
    options.BotToken = env["BotToken"] ?? throw new DataException("Settings must be initialized.");
    options.ChatId = env["ChatId"] ?? throw new DataException("Settings must be initialized.");
});

builder.Services.AddSingleton<IPollyService, PollyService>();

builder.Services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://api.telegram.org/");
    })
    .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpRetryPolicy())
    .AddPolicyHandler((provider, _) => provider.GetService<IPollyService>()!.GetHttpCircuitBreakerPolicy());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
