namespace InvestLens.Worker.Services;

public interface ISecuritiesService
{
    Task ProcessDailyDataRefreshAsync();
    Task InitializeApplicationAsync();
}