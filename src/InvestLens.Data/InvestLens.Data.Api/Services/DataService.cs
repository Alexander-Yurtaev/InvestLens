using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Models.Settings;
using InvestLens.Data.Entities;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Helpers;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.Data.Api.Services;

public class DataService : IDataService
{
    private readonly ICommonSettings _commonSettings;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly IMessageBusClient _messageBus;

    public DataService(
        ICommonSettings commonSettings,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        IMessageBusClient messageBus)
    {
        _commonSettings = commonSettings;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _messageBus = messageBus;
    }


    public async Task<IGetResult<Security, Guid>> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "")
    {
        await RefreshSecurities();
        return await _securityRepository.Get(page, pageSize, sort, filter);
    }

    private async Task RefreshSecurities()
    {
        var refreshStatus = await _refreshStatusRepository.GetRefreshStatus(DatabaseConstants.SecurityEntityName);
        if (refreshStatus is null || !DateTimeHelper.IsRefreshed(refreshStatus.RefreshDate, _commonSettings.ExpiredRefreshStatusMinutes))
        {
            var message = new SecurityRefreshMessage();
            await _messageBus.PublishAsync(message, BusClientConstants.SecuritiesExchangeName, BusClientConstants.SecuritiesRefreshKey);
        }   
    }
}