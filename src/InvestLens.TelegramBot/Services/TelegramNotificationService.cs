using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace InvestLens.TelegramBot.Services;

using Models;

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        HttpClient httpClient,
        IOptions<TelegramSettings> settings,
        ILogger<TelegramNotificationService> logger)
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

    public async Task NotifyErrorAsync(string operation, Exception exception, CancellationToken cancellationToken = default)
    {
        var message = $"❌ <b>Ошибка в операции</b>\n" +
                     $"Операция: {operation}\n" +
                     $"Ошибка: {exception.Message}\n" +
                     $"Время: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await NotifyAsync(message, cancellationToken);
    }

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
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram notification.");
            throw;
        }
    }
}
