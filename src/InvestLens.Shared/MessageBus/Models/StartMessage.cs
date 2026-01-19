namespace InvestLens.Shared.MessageBus.Models;

public class StartMessage : BaseInformationMessage
{
    public StartMessage(string operationId) : base(operationId)
    {
    }

    public string Details { get; set; } = string.Empty;
}