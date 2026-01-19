namespace InvestLens.Shared.MessageBus.Models;

public abstract class BaseInformationMessage : BaseTelegramMessage
{
    protected BaseInformationMessage(string operationId) : base(operationId)
    {
    }
}