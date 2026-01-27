namespace InvestLens.Shared.MessageBus.Models;

public abstract class BaseInformationMessage : BaseTelegramMessage
{
    protected BaseInformationMessage(Guid correlationId) : base(correlationId)
    {
    }
}