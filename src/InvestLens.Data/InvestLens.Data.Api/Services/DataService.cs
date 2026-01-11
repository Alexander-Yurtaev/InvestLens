using InvestLens.Abstraction.MessageBus;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Models;
using InvestLens.Data.Entities;
using InvestLens.Data.Repositories;

namespace InvestLens.Data.Api.Services;

public class DataService : IDataService
{
    private const string EntityName = "SECURITIES";
    private const string ExchangeName = "securities-exchange";

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
        var expiredRefreshStatusString = _configuration["EXPIRED_REFRESH_STATUS"];
        if (string.IsNullOrEmpty(expiredRefreshStatusString))
        {
            throw new InvalidDataException("EXPIRED_REFRESH_STATUS");
        }

        var expiredRefreshStatus = int.Parse(expiredRefreshStatusString);

        var refreshStatus = await _refreshStatusRepository.GetRefreshStatus(EntityName);
        if (refreshStatus is null || (refreshStatus.RefreshDate.AddHours(expiredRefreshStatus) < DateTime.UtcNow))
        {
            var message = new SecurityRefreshMessage();
            string routingKey = "securities.refresh";

            await _messageBus.PublishAsync(message, ExchangeName, routingKey);
        }   
    }
}