namespace InvestLens.Data.Api.Models.Settings;

public interface ICommonSettings
{
    string TargetMigration { get; init; }
    string MoexBaseUrl { get; init; }
    TimeSpan ExpiredRefreshStatus { get; init; }
}