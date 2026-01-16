namespace InvestLens.Data.Api.Models.Settings;

public interface ICommonSettings
{
    string TargetMigration { get; init; }
    string MoexBaseUrl { get; init; }
    int ExpiredRefreshStatusMinutes { get; init; }
}