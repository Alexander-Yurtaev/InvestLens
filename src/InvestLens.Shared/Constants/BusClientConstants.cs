namespace InvestLens.Shared.Constants;

public static class BusClientConstants
{
    public const string SecuritiesExchangeName = "securities-exchange";
    public const string GlobalIssDictionariesExchangeName = "global-iss-dictionaries-exchange";
    public const string TelegramExchangeName = "telegram-exchange";

    public const string WorkerSecuritiesRefreshKey = "worker.securities.refresh"; // Send command to Worker
    public const string DataSecuritiesRefreshKey = "data.securities.refresh"; // Send command to Data

    public const string WorkerGlobalIssDictionariesRefreshKey = "worker.securities.refresh"; // Send command to Worker
    public const string DataGlobalIssDictionariesRefreshKey = "data.global.iss.dictionaries.refresh"; // Send command to Data

    public const string TelegramStartKey = "telegram.start";
    public const string TelegramCompleteKey = "telegram.complete";
    public const string TelegramErrorKey = "telegram.error";

    public const string SecretesRefreshQueue = "securities-refresh-queue";
    public const string GlobalIssDictionariesRefreshQueue = "global-iss-dictionaries-refresh-queue";
    public const string TelegramStartQueue = "telegram-start-queue";
    public const string TelegramCompleteQueue = "telegram-complete-queue";
    public const string TelegramErrorQueue = "telegram-error-queue";
}