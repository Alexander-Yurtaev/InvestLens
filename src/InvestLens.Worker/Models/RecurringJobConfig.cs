namespace InvestLens.Worker.Models;

public class RecurringJobConfig
{
    public string JobId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string Queue { get; set; } = "default";
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;

    public TimeZoneInfo GetTimeZoneInfo() =>
        TimeZoneInfo.FindSystemTimeZoneById(TimeZone) ?? TimeZoneInfo.Utc;
}