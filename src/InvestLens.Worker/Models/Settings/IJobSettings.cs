namespace InvestLens.Worker.Models.Settings;

public interface IJobSettings
{
    public int MaxRetryCount { get; init; }
    public int DelayBetweenRefresh { get; init; }
}