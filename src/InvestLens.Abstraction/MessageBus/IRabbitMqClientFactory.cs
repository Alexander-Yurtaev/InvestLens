namespace InvestLens.Abstraction.MessageBus;

public interface IRabbitMqClientFactory
{
    Task<IMessageBusClient> CreateRabbitMqClient(CancellationToken cancellationToken);
}