using InvestLens.Abstraction.MessageBus.Models;

namespace InvestLens.Abstraction.MessageBus.Services;

public interface IMessageHandler<in T> where T : IBaseMessage
{
    Task<bool> HandleAsync(T message, CancellationToken cancellationToken = default);
}