using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using RabbitMQ.Client.Exceptions;
using StackExchange.Redis;
using System.Net.Sockets;

namespace InvestLens.Shared.Services;

public class PollyService : IPollyService
{
    private readonly ILogger<PollyService> _logger;

    public PollyService(ILogger<PollyService> logger)
    {
        _logger = logger;
    }

    // HTTP политики
    public IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (result, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        "HTTP Retry {RetryAttempt} after {Seconds}s. Status: {StatusCode}",
                        retryAttempt, timespan.TotalSeconds,
                        result.Result?.StatusCode ?? 0);
                });
    }

    // RabbitMQ политики
    public AsyncPolicy GetRabbitMqRetryPolicy()
    {
        return Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .Or<TimeoutException>()
            .Or<OperationInterruptedException>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(5 * retryAttempt), // 5, 10, 15, 20, 25 сек
                onRetry: (exception, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "RabbitMQ Retry {RetryAttempt} after {Seconds}s",
                        retryAttempt, timespan.TotalSeconds);
                });
    }

    public IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5, // Увеличил с 3 до 5
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, breakDelay) =>
                {
                    _logger.LogError(
                        "HTTP Circuit Breaker открыт на {Seconds}s. Status: {StatusCode}",
                        breakDelay.TotalSeconds,
                        result.Result?.StatusCode ?? 0);
                },
                onReset: () =>
                {
                    _logger.LogInformation("HTTP Circuit Breaker сброшен");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("HTTP Circuit Breaker в Half-Open состоянии");
                });
    }

    public AsyncPolicy GetRabbitMqCircuitBreakerPolicy()
    {
        return Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .Or<TimeoutException>()
            .Or<OperationInterruptedException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 15, // Должно быть > retryCount!
                durationOfBreak: TimeSpan.FromSeconds(60),
                onBreak: (exception, breakDelay) =>
                {
                    _logger.LogError(
                        exception,
                        "RabbitMQ Circuit Breaker открыт на {Seconds}s",
                        breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("RabbitMQ Circuit Breaker сброшен");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("RabbitMQ Circuit Breaker в Half-Open состоянии");
                });
    }

    // Комбинированные политики с правильным порядком
    public IAsyncPolicy<HttpResponseMessage> GetHttpResilientPolicy()
    {
        var retryPolicy = GetHttpRetryPolicy();
        var circuitBreakerPolicy = GetHttpCircuitBreakerPolicy();

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    public AsyncPolicy GetRabbitMqResilientPolicy()
    {
        var retryPolicy = GetRabbitMqRetryPolicy();
        var circuitBreakerPolicy = GetRabbitMqCircuitBreakerPolicy();

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    // Универсальный метод с правильными настройками
    public AsyncPolicy GetResilientPolicy<TException>(
        int retryCount = 5,
        int circuitBreakerThreshold = 10)
        where TException : Exception
    {
        var retryPolicy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(5 * retryAttempt),
                onRetry: (exception, timespan, rc, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Попытка {RetryCount} после исключения {ExceptionType}. Ожидание {Seconds} секунд...",
                        rc, typeof(TException).Name, timespan.TotalSeconds);
                });

        var circuitBreakerPolicy = Policy
            .Handle<TException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: circuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, breakDelay) =>
                {
                    _logger.LogError(
                        exception,
                        "Цепь разомкнута на {Seconds} секунд после исключения {ExceptionType}!",
                        breakDelay.TotalSeconds, typeof(TException).Name);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Цепь для {ExceptionType} восстановлена!", typeof(TException).Name);
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Цепь для {ExceptionType} в состоянии тестирования...", typeof(TException).Name);
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    // Дополнительно: политика с экспоненциальными backoff для RabbitMQ
    public AsyncPolicy GetRabbitMqExponentialBackoffPolicy()
    {
        return Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 6,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8, 16, 32, 64 сек
                onRetry: (_, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        "RabbitMQ: Попытка {Attempt} через {Seconds}s",
                        retryAttempt, timespan.TotalSeconds);
                })
            .WrapAsync(
                Policy
                    .Handle<BrokerUnreachableException>()
                    .Or<SocketException>()
                    .Or<TimeoutException>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: 12,
                        durationOfBreak: TimeSpan.FromMinutes(2)));
    }

    public AsyncPolicy GetRedisResilientPolicy()
    {
        var retryPolicy = Policy
            .Handle<RedisException>() // Если у вас есть такой тип
            .Or<TimeoutException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount: 3, // Redis обычно быстрый - меньше попыток
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(100 * retryAttempt), // Быстрые retry
                onRetry: (exception, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Redis Retry {RetryAttempt} after {Milliseconds}ms",
                        retryAttempt, timespan.TotalMilliseconds);
                });

        var circuitBreakerPolicy = Policy
            .Handle<RedisException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(10), // Короткое время для Redis
                onBreak: (exception, breakDelay) =>
                {
                    _logger.LogError(
                        exception,
                        "Redis Circuit Breaker opened for {Seconds}s",
                        breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Redis Circuit Breaker reset");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Redis Circuit Breaker half-open");
                });

        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);
    }

    // Универсальный метод для любых типов (не только string)
    public IAsyncPolicy<T> GetGenericResilientPolicy<T>()
    {
        var retryPolicy = Policy<T>
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .OrResult(result => result == null) // Проверка на null
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (result, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        "Generic Retry {RetryAttempt} after {Seconds}s. Type: {Type}, Error: {Error}",
                        retryAttempt, timespan.TotalSeconds, typeof(T).Name,
                        result.Exception?.Message ?? "Result validation failed");
                });

        var circuitBreakerPolicy = Policy<T>
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, breakDelay) =>
                {
                    _logger.LogError(
                        "Generic Circuit Breaker opened for {Seconds}s. Type: {Type}, Error: {Error}",
                        breakDelay.TotalSeconds, typeof(T).Name,
                        result.Exception?.Message ?? "Unknown error");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Generic Circuit Breaker for {Type} reset", typeof(T).Name);
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Generic Circuit Breaker for {Type} half-open", typeof(T).Name);
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}