namespace InvestLens.Shared.Messages;

public class StartMessage : BaseInformationMessage
{
    public string Details { get; set; } = string.Empty;
}