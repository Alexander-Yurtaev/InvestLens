using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace InvestLens.Shared.Services;

public class PollyService : IPollyService
{
    private readonly ILogger<PollyService> _logger;

    public PollyService(ILogger<PollyService> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Обрабатывает 5xx и 408 ошибки
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Экспоненциальная задержка
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning($"Повторная попытка {retryAttempt} через {timespan.TotalSeconds} секунд...");
                });
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    _logger.LogWarning($"Circuit Breaker открыт на {breakDelay.TotalSeconds} секунд");
                },
                onReset: () =>
                {
                    _logger.LogWarning("Circuit Breaker сброшен");
                });
    }
}