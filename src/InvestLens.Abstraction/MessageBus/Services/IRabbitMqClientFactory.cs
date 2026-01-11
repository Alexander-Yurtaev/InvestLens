namespace InvestLens.Abstraction.MessageBus.Services;

public interface IRabbitMqClientFactory
{
    Task<IMessageBusClient> CreateRabbitMqClient(CancellationToken cancellationToken);
}