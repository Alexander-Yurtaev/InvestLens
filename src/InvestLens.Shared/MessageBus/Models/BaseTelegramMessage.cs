namespace InvestLens.Shared.MessageBus.Models;

public abstract class BaseTelegramMessage : BaseMessage
{
    public BaseTelegramMessage(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; init; }
}