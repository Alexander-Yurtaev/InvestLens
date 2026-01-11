using InvestLens.Abstraction.MessageBus;
using InvestLens.Abstraction.Repositories;
using InvestLens.Abstraction.Services;
using InvestLens.Data.Api.Converter;
using InvestLens.Data.Api.Models;

namespace InvestLens.Data.Api.Handlers;

public class SecurityRefreshEventHandler : IMessageHandler<SecurityRefreshMessage>
{
    private const string EntityName = "SECURITIES";

    private readonly IMoexClient _moexClient;
    private readonly ISecurityRepository _securityRepository;
    private readonly IRefreshStatusRepository _refreshStatusRepository;
    private readonly ILogger<SecurityRefreshEventHandler> _logger;

    public SecurityRefreshEventHandler(
        IMoexClient moexClient,
        ISecurityRepository securityRepository,
        IRefreshStatusRepository refreshStatusRepository,
        ILogger<SecurityRefreshEventHandler> logger)
    {
        _moexClient = moexClient;
        _securityRepository = securityRepository;
        _refreshStatusRepository = refreshStatusRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(SecurityRefreshMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получено поручение обновить список ценных бумаг.");

        try
        {
            await RefreshSecurities();
            _logger.LogInformation("Cписок ценных бумаг обновлен.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении списка ценных бумаг.");
            return false;
        }
    }

    private async Task RefreshSecurities()
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
}