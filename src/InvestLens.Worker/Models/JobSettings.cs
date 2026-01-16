namespace InvestLens.Worker.Models;

public class JobSettings
{
    public int MaxRetryCount { get; set; }
    public int DelayBetweenRefresh { get; set; }
}