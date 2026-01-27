using InvestLens.Abstraction.Services;

namespace InvestLens.TelegramBot
{
    [Obsolete("This is an example class.")]
    public class Worker : BackgroundService
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<Worker> _logger;

        public Worker(ITelegramBotClient telegramBotClient, ILogger<Worker> logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var correlationId = Guid.NewGuid();

                try
                {
                    // Уведомление о начале
                    _logger.LogInformation("Запуск длинной операции обработки данных {OperationId}", correlationId);
                    await _telegramBotClient.NotifyOperationStartAsync(
                        correlationId,
                        "Запуск длинной операции обработки данных",
                        stoppingToken);

                    var startTime = DateTime.UtcNow;

                    // Долгая операция
                    await PerformLongOperation(correlationId, stoppingToken);

                    var duration = DateTime.UtcNow - startTime;

                    // Уведомление об окончании
                    _logger.LogInformation("Операция успешно завершена {OperationId}", correlationId);
                    await _telegramBotClient.NotifyOperationCompleteAsync(
                        correlationId,
                        "Операция успешно завершена",
                        duration,
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении длинной операции {CorrelationId}", correlationId);
                    await _telegramBotClient.NotifyErrorAsync(correlationId, $"Длинная операция {correlationId}: {ex.Message}", stoppingToken);
                }

                _logger.LogInformation("Операция завершена {CorrelationId}", correlationId);
                await _telegramBotClient.NotifyAsync($"Операция завершена {correlationId}", stoppingToken);

                break;
            }
        }

        private async Task PerformLongOperation(Guid correlationId, CancellationToken cancellationToken)
        {
            // Имитация длительной операции
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}