using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Data.Entities;

namespace InvestLens.Data.Api.Services;

public class DataService : IDataService
{
    private const string EntityName = "SECURITIES";

    private readonly IMoexClient _moexClient;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly IConfiguration _configuration;

    public DataService(IMoexClient moexClient, 
        ISecurityRepository securityRepository, 
        IRefreshStatusRepository refreshStatusRepository,
        IConfiguration configuration)
    {
        _moexClient = moexClient;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _configuration = configuration;
    }


    public async Task<List<Security>> GetSecurities()
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
            // 1. Получаем данные от MOEX
            var securitiesResponse = await _moexClient.GetSecurities();
            if (securitiesResponse is null || securitiesResponse.Securities.Data.Length == 0)
            {
                throw new InvalidOperationException("Не были получены данные от MOEX.");
            }

            // 2. Конвертируем данные из Response в Entity
            var securities = ResponseToEntityConverters.SecurityResponseToEntityConverter(securitiesResponse);

            // 3. Сохраняем/обновляем данные в БД
            await _securityRepository.Add(securities, true);

            // 4. Обновляем RefreshStatus
            await _refreshStatusRepository.SetRefreshStatus(EntityName);
        }

        return await _securityRepository.Get();
    }
}