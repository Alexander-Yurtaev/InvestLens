namespace InvestLens.Shared.MessageBus.Models;

public class CompleteMessage : BaseInformationMessage
{
    public CompleteMessage(string operationId) : base(operationId)
    {
    }

    public TimeSpan Duration => FinishedAt.HasValue ? FinishedAt.Value - CreatedAt : TimeSpan.Zero;

    public int Count { get; set; }
}