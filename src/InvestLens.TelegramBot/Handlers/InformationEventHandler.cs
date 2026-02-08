using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Telegram.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Interfaces.Services;
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
                "Пришло сообщение Id={MessageId} без CorrelationId. Новый correlationId: {CorrelationId}",
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
        _logger.LogInformation("Операция началась, MessageId: {MessageId} .", message.MessageId);
        await _telegramBotClient.NotifyOperationStartAsync(message.Details, cancellationToken);
        return await Task.FromResult(true);
    }

    private async Task<bool> HandleCompleteMessageAsync(CompleteMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Операция завершилась: за {Duration:hh\\:mm\\:ss} было скачано {Count} записей, MessageId: {MessageId} ",
            message.Duration, message.Count, message.MessageId);

        await _telegramBotClient.NotifyOperationCompleteAsync(
            $"Операция завершилась: за {message.Duration:hh\\:mm\\:ss} было скачано {message.Count} записей, MessageId: {message.MessageId}",
            message.Duration, cancellationToken);

        return await Task.FromResult(true);
    }

    #endregion Private Methods
}