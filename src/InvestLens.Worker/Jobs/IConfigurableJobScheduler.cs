using InvestLens.Worker.Models;

namespace InvestLens.Worker.Jobs;

public interface IConfigurableJobScheduler
{
    void ScheduleRecurringJobs();
    void ScheduleStartupJobs();
    void UpdateJobSchedule(string jobId, string newCronExpression);
    List<RecurringJobConfig> GetRegisteredJobs();
}