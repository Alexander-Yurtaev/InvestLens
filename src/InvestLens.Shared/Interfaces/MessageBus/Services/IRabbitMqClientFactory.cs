using InvestLens.Abstraction.MessageBus.Services;

namespace InvestLens.Shared.Interfaces.MessageBus.Services;

public interface IRabbitMqClientFactory
{
    Task<IMessageBusClient> CreateRabbitMqClient(CancellationToken cancellationToken);
}