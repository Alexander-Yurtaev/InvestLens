using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.TelegramBot.Handlers;

public class ErrorEventHandler : IMessageHandler<ErrorMessage>
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ILogger<ErrorEventHandler> _logger;

    public ErrorEventHandler(ITelegramNotificationService telegramNotificationService, ILogger<ErrorEventHandler> logger)
    {
        _telegramNotificationService = telegramNotificationService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ErrorMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Операция {OperationId} завершилась неудачно: {Exception}", message.OperationId, message.Exception.Message);
        await _telegramNotificationService.NotifyOperationStartAsync(message.OperationId,
            $"Операция {message.OperationId} завершилась неудачно: {message.Exception.Message}", cancellationToken);
        return await Task.FromResult(true);
    }
}