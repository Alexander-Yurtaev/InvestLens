using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.MessageBus.Models;

namespace InvestLens.Worker.Services;

public class SecuritiesService : ISecuritiesService
{
    private readonly IMessageBusClient _messageBusClient;
    private readonly IPollyService _pollyService;
    private readonly ILogger<SecuritiesService> _logger;
    private readonly CancellationToken _cancellationToken;

    public SecuritiesService(IMessageBusClient messageBusClient, IPollyService pollyService, ILogger<SecuritiesService> logger)
    {
        _messageBusClient = messageBusClient;
        _pollyService = pollyService;
        _logger = logger;
        _cancellationToken = CancellationToken.None;
    }

    public async Task ProcessDailyDataRefreshAsync()
    {
        _logger.LogInformation("Запуск ежедневного обновления списка ценных бумаг.");
        try
        {
            await RefreshSecuritiesAsync();
            _logger.LogInformation("Ежедневное обновление списка ценных бумаг завершено успешно.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ежедневное обновление списка ценных бумаг завершилось с ошибкой.");
            throw;
        }
    }

    public async Task InitializeApplicationAsync()
    {
        _logger.LogInformation("Запуск обновления списка ценных бумаг при инициализации приложения.");
        try
        {
            await RefreshSecuritiesAsync();
            _logger.LogInformation("Обновление списка ценных бумаг при инициализации приложения завершено успешно.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Обновление списка ценных бумаг при инициализации приложения завершилось с ошибкой.");
            throw;
        }
    }

    #region Private Methods

    private async Task RefreshSecuritiesAsync()
    {
        var resilientPolicy = _pollyService.GetRabbitMqResilientPolicy();
        await resilientPolicy.ExecuteAndCaptureAsync(async () =>
        {
            await _messageBusClient.PublishAsync(new SecurityRefreshMessage(),
                                                 BusClientConstants.SecuritiesExchangeName,
                                                 BusClientConstants.DataSecuritiesRefreshKey,
                                                 _cancellationToken);
        });
    }

    #endregion Private Methods
}