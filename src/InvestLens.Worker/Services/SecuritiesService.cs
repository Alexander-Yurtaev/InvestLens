namespace InvestLens.Worker.Services;

public class SecuritiesService : ISecuritiesService
{
    private readonly ILogger<SecuritiesService> _logger;

    public SecuritiesService(ILogger<SecuritiesService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessDailyDataRefreshAsync()
    {
        await RefreshSecuritiesAsync();
    }

    public async Task InitializeApplicationAsync()
    {
        await RefreshSecuritiesAsync();
    }

    #region Private Methods

    private async Task RefreshSecuritiesAsync()
    {
        _logger.LogDebug("Send message to the RabbitMQ to refresh securities.");
        await Task.CompletedTask;
    }

    #endregion Private Methods
}