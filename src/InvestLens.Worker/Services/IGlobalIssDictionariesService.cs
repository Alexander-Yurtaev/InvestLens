namespace InvestLens.Worker.Services;

public interface IGlobalIssDictionariesService
{
    Task ProcessDailyDataRefreshAsync();
    Task InitializeApplicationAsync();
}