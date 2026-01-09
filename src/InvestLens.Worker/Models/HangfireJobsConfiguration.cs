namespace InvestLens.Worker.Models;

public class HangfireJobsConfiguration
{
    public List<RecurringJobConfig> RecurringJobs { get; set; } = new();
    public List<StartupJobConfig> StartupJobs { get; set; } = new();
}