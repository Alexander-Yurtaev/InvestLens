namespace InvestLens.Data.Api.Models.Settings;

public class CommonSettings : ICommonSettings
{
    public string TargetMigration { get; init; } = string.Empty;
    public string MoexBaseUrl { get; init; } = string.Empty;
    public int ExpiredRefreshStatusMinutes { get; init; }
}
