using Polly;
using Polly.Wrap;

namespace InvestLens.Abstraction.Services;

public interface IPollyService
{
    IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy();
    IAsyncPolicy<HttpResponseMessage> GetHttpResilientPolicy();
    IAsyncPolicy<HttpResponseMessage> GetRabbitMqRetryPolicy();
    AsyncPolicyWrap GetResilientPolicy<TException>() where TException : Exception;
}