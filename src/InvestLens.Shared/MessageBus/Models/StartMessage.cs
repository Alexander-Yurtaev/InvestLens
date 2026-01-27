namespace InvestLens.Shared.MessageBus.Models;

public class StartMessage : BaseInformationMessage
{
    public StartMessage(Guid correlationId) : base(correlationId)
    {
    }

    public string Details { get; set; } = string.Empty;
}