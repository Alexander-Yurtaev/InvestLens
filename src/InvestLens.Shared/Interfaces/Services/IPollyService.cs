using Polly;

namespace InvestLens.Shared.Interfaces.Services;

public interface IPollyService
{
    IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy();
    AsyncPolicy GetRabbitMqRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy();
    AsyncPolicy GetRabbitMqCircuitBreakerPolicy();
    IAsyncPolicy<HttpResponseMessage> GetHttpResilientPolicy();
    AsyncPolicy GetRabbitMqResilientPolicy();

    AsyncPolicy GetResilientPolicy<TException>(
        int retryCount = 5,
        int circuitBreakerThreshold = 10)
        where TException : Exception;

    AsyncPolicy GetRabbitMqExponentialBackoffPolicy();

    AsyncPolicy GetRedisResilientPolicy();

    IAsyncPolicy<T> GetGenericResilientPolicy<T>();
}