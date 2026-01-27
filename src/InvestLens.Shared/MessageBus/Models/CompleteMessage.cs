namespace InvestLens.Shared.MessageBus.Models;

public class CompleteMessage : BaseInformationMessage
{
    public CompleteMessage(Guid correlationId) : base(correlationId)
    {
    }

    public TimeSpan Duration => FinishedAt.HasValue ? FinishedAt.Value - CreatedAt : TimeSpan.Zero;

    public int Count { get; set; }
}