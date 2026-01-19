namespace InvestLens.Shared.Constants;

public static class BusClientConstants
{
    public const string SecuritiesExchangeName = "securities-exchange";
    public const string TelegramExchangeName = "telegram-exchange";

    public const string SecuritiesRefreshKey = "securities.refresh";
    public const string SecuritiesRefreshingKey = "securities.refreshing";

    public const string TelegramStartKey = "telegram.start";
    public const string TelegramCompleteKey = "telegram.complete";
    public const string TelegramErrorKey = "telegram.error";

    public const string SecretesRefreshQueue = "securities-refresh-queue";
    public const string TelegramStartQueue = "telegram-start-queue";
    public const string TelegramCompleteQueue = "telegram-complete-queue";
    public const string TelegramErrorQueue = "telegram-error-queue";
}