using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Interfaces.Telegram.Services;
using InvestLens.Shared.Messages;
using Serilog.Context;

namespace InvestLens.TelegramBot.Handlers;

public class InformationEventHandler : IMessageHandler<BaseInformationMessage>
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<InformationEventHandler> _logger;

    public InformationEventHandler(ITelegramBotClient telegramBotClient, ICorrelationIdService correlationIdService, ILogger<InformationEventHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(BaseInformationMessage message, CancellationToken cancellationToken = default)
    {
        var correlationId = message.Headers[HeaderConstants.CorrelationHeader].ToString() ?? "";
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = _correlationIdService.GetOrCreateCorrelationId("TelegramBot");
            _logger.LogWarning(
                "I received a message Id={messageId} without a correlationId. New correlationId: {correlationId}",
                message.MessageId, correlationId);
        }
        else
        {
            _correlationIdService.SetCorrelationId(correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            return message switch
            {
                StartMessage startMessage => await HandleStartMessageAsync(startMessage, cancellationToken),
                CompleteMessage completeMessage => await HandleCompleteMessageAsync(completeMessage, cancellationToken),
                _ => false
            };
        }
    }

    #region Private Methods

    private async Task<bool> HandleStartMessageAsync(StartMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("The operation has started, messageId: {messageId}.", message.MessageId);
        await _telegramBotClient.NotifyOperationStartAsync(message, message.Details, cancellationToken);
        return await Task.FromResult(true);
    }

    private async Task<bool> HandleCompleteMessageAsync(CompleteMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Operation completed: {Duration:hh\\:mm\\:ss} {Count} records were downloaded, messageId: {messageId}",
            message.Duration, message.Count, message.MessageId);

        await _telegramBotClient.NotifyOperationCompleteAsync(message,
            $"Operation completed: for {message.Duration:hh\\:mm\\:ss} was downloaded {message.Count} entities, MessageId: {message.MessageId}",
        message.Duration, cancellationToken);

        return await Task.FromResult(true);
    }

    #endregion Private Methods
}