using Polly;

namespace InvestLens.Abstraction.Services;

public interface IPollyService
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
}