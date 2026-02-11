using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Interfaces.Telegram.Services;
using InvestLens.Shared.Messages;
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
                "I received a message Id={messageId} without a correlationId. New correlationId: {CorrelationId}",
                message.MessageId, correlationId);
        }
        else
        {
            _correlationIdService.SetCorrelationId(correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("The operation failed: {Exception}", message.Exception.Message);

            await _telegramBotClient.NotifyErrorAsync(message, $"The operation failed: {message.Exception.Message}",
                cancellationToken);

            return await Task.FromResult(true);
        }
    }
}