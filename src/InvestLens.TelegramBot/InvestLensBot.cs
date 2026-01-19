using InvestLens.Abstraction.Services;

namespace InvestLens.TelegramBot
{
    public class InvestLensBot : BackgroundService
    {
        private readonly ITelegramNotificationService _telegramService;
        private readonly ILogger<InvestLensBot> _logger;

        public InvestLensBot(ITelegramNotificationService telegramService, ILogger<InvestLensBot> logger)
        {
            _telegramService = telegramService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var operationId = Guid.NewGuid().ToString();

                try
                {
                    // Уведомление о начале
                    _logger.LogInformation("Запуск длинной операции обработки данных {OperationId}", operationId);
                    await _telegramService.NotifyOperationStartAsync(
                        operationId,
                        "Запуск длинной операции обработки данных",
                        stoppingToken);

                    var startTime = DateTime.UtcNow;

                    // Долгая операция
                    await PerformLongOperation(operationId, stoppingToken);

                    var duration = DateTime.UtcNow - startTime;

                    // Уведомление об окончании
                    _logger.LogInformation("Операция успешно завершена {OperationId}", operationId);
                    await _telegramService.NotifyOperationCompleteAsync(
                        operationId,
                        "Операция успешно завершена",
                        duration,
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении длинной операции {OperationId}", operationId);
                    await _telegramService.NotifyErrorAsync($"Длинная операция {operationId}", ex, stoppingToken);
                }

                _logger.LogInformation("Операция завершена {OperationId}", operationId);
                await _telegramService.NotifyAsync($"Операция завершена {operationId}", stoppingToken);

                break;
            }
        }

        private async Task PerformLongOperation(string operationId, CancellationToken cancellationToken)
        {
            // Имитация длительной операции
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}
