using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Entities;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.Data.Api.Services;

public class DataService : IDataService
{
    private readonly IConfiguration _configuration;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly IMessageBusClient _messageBus;

    public DataService(
        IConfiguration configuration,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        IMessageBusClient messageBus)
    {
        _configuration = configuration;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _messageBus = messageBus;
    }


    public async Task<List<Security>> GetSecurities()
    {
        await RefreshSecurities();
        return await _securityRepository.Get();
    }

    private async Task RefreshSecurities()
    {
        var expiredRefreshStatusString = _configuration["ExpiredRefreshStatus"];
        if (string.IsNullOrEmpty(expiredRefreshStatusString))
        {
            throw new InvalidDataException("ExpiredRefreshStatus");
        }

        var expiredRefreshStatus = int.Parse(expiredRefreshStatusString);

        var refreshStatus = await _refreshStatusRepository.GetRefreshStatus(DatabaseConstants.SecurityEntityName);
        if (refreshStatus is null || !DateTimeHelper.IsRefreshed(refreshStatus.RefreshDate, expiredRefreshStatus))
        {
            var message = new SecurityRefreshMessage();
            await _messageBus.PublishAsync(message, BusClientConstants.ExchangeName, BusClientConstants.SecuritiesRefreshKey);
        }   
    }
}