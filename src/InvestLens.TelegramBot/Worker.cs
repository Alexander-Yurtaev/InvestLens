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
                var operationId = Guid.NewGuid().ToString();

                try
                {
                    // Уведомление о начале
                    _logger.LogInformation("Запуск длинной операции обработки данных {OperationId}", operationId);
                    await _telegramBotClient.NotifyOperationStartAsync(
                        operationId,
                        "Запуск длинной операции обработки данных",
                        stoppingToken);

                    var startTime = DateTime.UtcNow;

                    // Долгая операция
                    await PerformLongOperation(operationId, stoppingToken);

                    var duration = DateTime.UtcNow - startTime;

                    // Уведомление об окончании
                    _logger.LogInformation("Операция успешно завершена {OperationId}", operationId);
                    await _telegramBotClient.NotifyOperationCompleteAsync(
                        operationId,
                        "Операция успешно завершена",
                        duration,
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении длинной операции {OperationId}", operationId);
                    await _telegramBotClient.NotifyErrorAsync($"Длинная операция {operationId}", ex.Message, stoppingToken);
                }

                _logger.LogInformation("Операция завершена {OperationId}", operationId);
                await _telegramBotClient.NotifyAsync($"Операция завершена {operationId}", stoppingToken);

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