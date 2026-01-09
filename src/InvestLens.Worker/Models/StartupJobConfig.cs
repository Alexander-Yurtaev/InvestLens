namespace InvestLens.Worker.Models;

public class StartupJobConfig
{
    public string JobId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public int DelaySeconds { get; set; } = 0;
    public string Queue { get; set; } = "default";
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}