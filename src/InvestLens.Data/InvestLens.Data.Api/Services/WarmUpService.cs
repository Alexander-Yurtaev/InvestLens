using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Data.Api.Services;

public class WarmUpService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WarmUpService> _logger;

    public WarmUpService(IServiceScopeFactory scopeFactory, 
        ILogger<WarmUpService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start to warm a cache.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dataReaderService = scope.ServiceProvider.GetRequiredService<IMoexDataReaderService>();

            await dataReaderService.GetAllSecurityTypes();
            await dataReaderService.GetAllSecurityGroups();
            await dataReaderService.GetSecuritiesWithDetails(1, 10);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }

        _logger.LogInformation("Finish to warm a cache.");
    }
}