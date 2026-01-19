namespace InvestLens.Shared.MessageBus.Models;

public abstract class BaseTelegramMessage : BaseMessage
{
    public BaseTelegramMessage(string operationId)
    {
        OperationId = operationId;
    }

    public string OperationId { get; init; }
}