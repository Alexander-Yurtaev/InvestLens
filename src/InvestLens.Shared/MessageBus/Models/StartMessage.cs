namespace InvestLens.Shared.MessageBus.Models;

public class StartMessage : BaseInformationMessage
{
    public string Details { get; set; } = string.Empty;
}