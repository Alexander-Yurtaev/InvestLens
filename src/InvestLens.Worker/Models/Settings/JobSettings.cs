namespace InvestLens.Worker.Models.Settings;

public class JobSettings : IJobSettings
{
    public int MaxRetryCount { get; init; }
    public int DelayBetweenRefresh { get; init; }
}