namespace InvestLens.Shared.Messages;

public class CompleteMessage : BaseInformationMessage
{
    public TimeSpan Duration => FinishedAt.HasValue ? FinishedAt.Value - CreatedAt : TimeSpan.Zero;

    public int Count { get; set; }
}