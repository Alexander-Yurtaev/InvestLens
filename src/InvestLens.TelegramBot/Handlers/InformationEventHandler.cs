using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.TelegramBot.Handlers;

public class InformationEventHandler : IMessageHandler<BaseInformationMessage>
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<InformationEventHandler> _logger;

    public InformationEventHandler(ITelegramBotClient telegramBotClient, ILogger<InformationEventHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(BaseInformationMessage message, CancellationToken cancellationToken = default)
    {
        return message switch
        {
            StartMessage startMessage => await HandleStartMessageAsync(startMessage, cancellationToken),
            CompleteMessage completeMessage => await HandleCompleteMessageAsync(completeMessage, cancellationToken),
            _ => false
        };
    }

    #region Private Methods

    private async Task<bool> HandleStartMessageAsync(StartMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Операция {OperationId} началась.", message.CorrelationId);
        await _telegramBotClient.NotifyOperationStartAsync(message.CorrelationId, message.Details, cancellationToken);
        return await Task.FromResult(true);
    }

    private async Task<bool> HandleCompleteMessageAsync(CompleteMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Операция {OperationId} завершилась: за {duration:hh\\:mm\\:ss} было скачано {Count} записей.",
            message.CorrelationId, message.Duration, message.Count);

        await _telegramBotClient.NotifyOperationCompleteAsync(message.CorrelationId,
            $"Операция {message.Count} завершилась. Было загружено {message.Count} записей.",
            message.Duration, cancellationToken);

        return await Task.FromResult(true);
    }

    #endregion Private Methods
}