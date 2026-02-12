using InvestLens.Abstraction.Telegram.Models;
using InvestLens.Shared.Interfaces.MessageBus.Models;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Interfaces.Telegram.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using InvestLens.Shared.Interfaces.Telegram.Models;

namespace InvestLens.TelegramBot.Services;

using Models;
using System.Threading;

public class TelegramBotClient : ITelegramBotClient
{
    private readonly HttpClient _httpClient;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramBotClient> _logger;

    public TelegramBotClient(
        HttpClient httpClient,
        IOptions<TelegramSettings> settings,
        ICorrelationIdService correlationIdService,
        ILogger<TelegramBotClient> logger)
    {
        _httpClient = httpClient;
        _correlationIdService = correlationIdService;
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

    public async Task NotifyOperationStartAsync(IBaseMessage baseMessage, string details, CancellationToken cancellationToken = default)
    {
        var message = $"🚀 <b>Операция начата</b>\n" +
                     $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                     $"Время создания: {baseMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Детали: {details}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyOperationCompleteAsync(IBaseMessage baseMessage, string result, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var message = $"✅ <b>Операция завершена</b>\n" +
                     $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                     $"Время создания: {baseMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Время завершения: {baseMessage.FinishedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Длительность: {duration}\n" +
                     $"Результат: {result}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyInfoAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var formattedMessage = $"ℹ️ <b>{title}</b>\n" +
                               $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                               $"{message}\n" +
                               $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(formattedMessage, cancellationToken);
    }

    public async Task NotifyErrorAsync(IBaseMessage baseMessage, string exceptionMessage, CancellationToken cancellationToken = default)
    {
        var message = $"❌ <b>Ошибка в операции</b>\n" +
                     $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                     $"Время создания: {baseMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Время завершения: {baseMessage.FinishedAt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"Ошибка: {exceptionMessage}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyWarningAsync(string warning, string details = "", CancellationToken cancellationToken = default)
    {
        var message = $"⚠️ <b>Внимание</b>\n" +
                      $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
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
                      $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                      $"Статус: {status}\n" +
                      $"Текущее состояние: {currentState}\n" +
                      $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyDataUpdateAsync(string dataType, int count, string description = "", CancellationToken cancellationToken = default)
    {
        var message = $"📊 <b>Обновление данных</b>\n" +
                      $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
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
                      $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
                      $"Задание: {taskName}\n" +
                      $"Результат: {result}\n" +
                      $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

    public async Task NotifyHeartbeatAsync(string serviceName, TimeSpan uptime, CancellationToken cancellationToken = default)
    {
        var message = $"❤️ <b>Heartbeat</b>\n" +
                      $"Correlation ID: {_correlationIdService.GetOrCreateCorrelationId(nameof(TelegramBotClient))}\n" +
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

        return await GetUpdatesWithRetryAsync<GetUpdatesResponse>("getUpdates", payload, cancellationToken);
    }

    public async Task<GetMeResponse?> GetMeAsync(CancellationToken cancellationToken)
    {
        return await GetUpdatesWithRetryAsync<GetMeResponse>("getMe", Array.Empty<object>(), cancellationToken);
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

    private async Task<T?> GetUpdatesWithRetryAsync<T>(string command, object payload, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/bot{_settings.BotToken}/{command}",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Telegram get data successfully");

            var updates = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
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
