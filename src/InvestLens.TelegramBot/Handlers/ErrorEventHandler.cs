using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Models;
using Serilog.Context;

namespace InvestLens.TelegramBot.Handlers;

public class ErrorEventHandler : IMessageHandler<ErrorMessage>
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<ErrorEventHandler> _logger;

    public ErrorEventHandler(ITelegramBotClient telegramBotClient, ICorrelationIdService correlationIdService, ILogger<ErrorEventHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ErrorMessage message, CancellationToken cancellationToken = default)
    {
        var correlationId = message.Headers[HeaderConstants.CorrelationHeader].ToString() ?? "";
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = _correlationIdService.GetOrCreateCorrelationId("TelegramBot");
            _logger.LogWarning(
                "Пришло сообщение Id={MessageId} без CorrelationId. Новый correlationId: {CorrelationId}",
                message.MessageId, correlationId);
        }
        else
        {
            _correlationIdService.SetCorrelationId(correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Операция завершилась неудачно: {Exception}", message.Exception.Message);

            await _telegramBotClient.NotifyErrorAsync($"Операция завершилась неудачно: {message.Exception.Message}",
                cancellationToken);

            return await Task.FromResult(true);
        }
    }
}