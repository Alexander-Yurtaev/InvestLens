using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Wrap;

namespace InvestLens.Shared.Services;

public class PollyService : IPollyService
{
    private readonly ILogger<PollyService> _logger;

    public PollyService(ILogger<PollyService> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Обрабатывает 5xx и 408 ошибки
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Экспоненциальная задержка
                onRetry: (_, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning($"Повторная попытка {retryAttempt} через {timespan.TotalSeconds} секунд...");
                });
    }

    public IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (_, breakDelay) =>
                {
                    _logger.LogWarning($"Circuit Breaker открыт на {breakDelay.TotalSeconds} секунд");
                },
                onReset: () =>
                {
                    _logger.LogWarning("Circuit Breaker сброшен");
                });
    }

    public IAsyncPolicy<HttpResponseMessage> GetHttpResilientPolicy()
    {
        var retryPolicy = GetHttpRetryPolicy();
        var circuitBreakerPolicy = GetHttpCircuitBreakerPolicy();
        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    public AsyncPolicyWrap GetResilientPolicy<TException>() where TException: Exception
    {
        // 1. Создаем политики
        var retryPolicy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(10*retryAttempt),
                onRetry: (_, timespan, retryCount, _) =>
                {
                    _logger.LogInformation($"Попытка {retryCount}. Ожидание {timespan.TotalSeconds} секунд...");
                });

        var circuitBreakerPolicy = Policy
            .Handle<TException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (_, breakDelay) =>
                {
                    _logger.LogInformation($"Цепь разомкнута на {breakDelay.TotalSeconds} секунд!");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Цепь восстановлена!");
                });

        // 2. Объединяем политики (сначала Retry, затем Circuit Breaker)
        var resilientPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        return resilientPolicy;
    }
}