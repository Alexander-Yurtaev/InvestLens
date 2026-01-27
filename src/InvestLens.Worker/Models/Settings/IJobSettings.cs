namespace InvestLens.Worker.Models.Settings;

public interface IJobSettings
{
    public int MaxRetryCount { get; init; }
    public TimeSpan DelayBetweenRefresh { get; init; }
}