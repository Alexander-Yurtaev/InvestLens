using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Validators;

public static class TelegramValidator
{
    public static void Validate(ConfigurationManager configuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(configuration["Telegram:BaseAddress"], "Telegram:BaseAddress");
        ArgumentException.ThrowIfNullOrEmpty(configuration["BOT_TOKEN"], "BOT_TOKEN");
        ArgumentException.ThrowIfNullOrEmpty(configuration["CHAT_ID"], "CHAT_ID");
    }
}