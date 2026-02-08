using InvestLens.Abstraction.MessageBus.Models;
using RabbitMQ.Client;

namespace InvestLens.Abstraction.MessageBus.Services;

public interface IMessageBusClient : IAsyncDisposable
{
    Task<IChannel> GetChannelAsync();

    Task PublishAsync<T>(
        T message,
        string exchangeName,
        string routingKey = "",
        CancellationToken cancellationToken = default)
        where T : IBaseMessage;

    Task SubscribeAsync<T, TH>(
        string queueName,
        string exchangeName = "",
        string routingKey = "",
        CancellationToken cancellationToken = default)
        where T : IBaseMessage
        where TH : IMessageHandler<T>;

    Task UnsubscribeAsync<T, TH>(CancellationToken cancellationToken = default)
        where T : IBaseMessage
        where TH : IMessageHandler<T>;
}