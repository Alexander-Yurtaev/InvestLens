using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace InvestLens.TelegramBot.Services;

using InvestLens.Abstraction.Models.Telegram;
using Models;
using System.Threading;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        HttpClient httpClient,
        IOptions<TelegramSettings> settings,
        ILogger<TelegramService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task NotifyAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.ChatId))
        {
            _logger.LogWarning("Telegram ChatId is not configured");
            return;
        }

        var payload = new
        {
            chat_id = _settings.ChatId,
            text = message,
            parse_mode = "HTML"
        };

        await SendWithRetryAsync(payload, cancellationToken);
    }

    public async Task NotifyOperationStartAsync(string operationId, string details, CancellationToken cancellationToken = default)
    {
        var message = $"🚀 <b>Операция начата</b>\n" +
                     $"ID: {operationId}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Детали: {details}";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyOperationCompleteAsync(string operationId, string result, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var message = $"✅ <b>Операция завершена</b>\n" +
                     $"ID: {operationId}\n" +
                     $"Длительность: {duration:hh\\:mm\\:ss}\n" +
                     $"Результат: {result}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyInfoAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var formattedMessage = $"ℹ️ <b>{title}</b>\n" +
                               $"{message}\n" +
                               $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(formattedMessage, cancellationToken);
    }

    public async Task NotifyErrorAsync(string operation, Exception exception, CancellationToken cancellationToken = default)
    {
        var message = $"❌ <b>Ошибка в операции</b>\n" +
                     $"Операция: {operation}\n" +
                     $"Ошибка: {exception.Message}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyWarningAsync(string warning, string details = "", CancellationToken cancellationToken = default)
    {
        var message = $"⚠️ <b>Внимание</b>\n" +
                     $"Предупреждение: {warning}\n";

        if (!string.IsNullOrEmpty(details))
        {
            message += $"Детали: {details}\n";
        }

        message += $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyStatusAsync(string status, string currentState, CancellationToken cancellationToken = default)
    {
        var message = $"📈 <b>Статус системы</b>\n" +
                     $"Статус: {status}\n" +
                     $"Текущее состояние: {currentState}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyDataUpdateAsync(string dataType, int count, string description = "", CancellationToken cancellationToken = default)
    {
        var message = $"📊 <b>Обновление данных</b>\n" +
                     $"Тип данных: {dataType}\n" +
                     $"Количество записей: {count}\n";

        if (!string.IsNullOrEmpty(description))
        {
            message += $"Описание: {description}\n";
        }

        message += $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyScheduledTaskAsync(string taskName, string result, CancellationToken cancellationToken = default)
    {
        var message = $"⏰ <b>Плановое задание выполнено</b>\n" +
                     $"Задание: {taskName}\n" +
                     $"Результат: {result}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyHeartbeatAsync(string serviceName, TimeSpan uptime, CancellationToken cancellationToken = default)
    {
        var message = $"❤️ <b>Heartbeat</b>\n" +
                     $"Сервис: {serviceName}\n" +
                     $"Аптайм: {uptime:dd\\.hh\\:mm\\:ss}\n" +
                     $"Статус: Работает нормально\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emoji"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <example>
    /// await NotifyCustomAsync("🔔", "Напоминание", "Пора проверить отчеты");
    /// await NotifyCustomAsync("🎉", "Поздравление", "Достигнута новая веха!");
    /// await NotifyCustomAsync("📋", "Заметка", "Не забудьте сделать backup");
    /// </example>
    public async Task NotifyCustomAsync(string emoji, string title, string message, CancellationToken cancellationToken = default)
    {
        var formattedMessage = $"{emoji} <b>{title}</b>\n" +
                               $"{message}\n" +
                               $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(formattedMessage, cancellationToken);
    }

    public async Task NotifyPlainAsync(string message, CancellationToken cancellationToken = default)
    {
        // Просто отправляем сообщение как есть, без форматирования
        await NotifyAsync(message, cancellationToken);
    }

    public async Task<GetUpdatesResponse?> GetUpdatesAsync(int nextUpdateId, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            offset = nextUpdateId,
            timeout = 20
        };

        return await GetUpdatesWithRetryAsync(payload, cancellationToken);
    }

    #region Private Methods

    private async Task SendWithRetryAsync(object payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/bot{_settings.BotToken}/sendMessage",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Telegram notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram notification.");
            throw;
        }
    }

    private async Task<GetUpdatesResponse?> GetUpdatesWithRetryAsync(object payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/bot{_settings.BotToken}/getUpdates",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Telegram updates get successfully");

            var updates = await response.Content.ReadFromJsonAsync<GetUpdatesResponse>(cancellationToken);
            return updates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Telegram updates.");
            throw;
        }
    }

    #endregion Private Methods
}
