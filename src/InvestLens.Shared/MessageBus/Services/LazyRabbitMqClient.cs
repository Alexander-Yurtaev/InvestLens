using InvestLens.Abstraction.MessageBus.Models;
using InvestLens.Abstraction.MessageBus.Services;
using RabbitMQ.Client;

namespace InvestLens.Shared.MessageBus.Services;

public class LazyRabbitMqClient : IMessageBusClient
{
    private readonly Lazy<Task<IMessageBusClient>> _lazyClient;

    public LazyRabbitMqClient(IRabbitMqClientFactory factory)
    {
        _lazyClient = new Lazy<Task<IMessageBusClient>>(() => factory.CreateRabbitMqClient(CancellationToken.None));
    }

    public async ValueTask DisposeAsync()
    {
        var client = await _lazyClient.Value;
        await client.DisposeAsync();
    }

    public async Task<IChannel> GetChannelAsync()
    {
        var client = await _lazyClient.Value;
        return await client.GetChannelAsync();
    }

    public async Task PublishAsync<T>(T message, string exchangeName, string routingKey = "",
        CancellationToken cancellationToken = default) where T : IBaseMessage
    {
        var client = await _lazyClient.Value;
        await client.PublishAsync(message, exchangeName, routingKey, cancellationToken);
    }

    public async Task SubscribeAsync<T, TH>(string queueName, string exchangeName = "", string routingKey = "",
        CancellationToken cancellationToken = default) where T : IBaseMessage where TH : IMessageHandler<T>
    {
        var client = await _lazyClient.Value;
        await client.SubscribeAsync<T, TH>(queueName, exchangeName, routingKey, cancellationToken);
    }

    public async Task UnsubscribeAsync<T, TH>(CancellationToken cancellationToken = default) where T : IBaseMessage where TH : IMessageHandler<T>
    {
        var client = await _lazyClient.Value;
        await client.UnsubscribeAsync<T, TH>(cancellationToken);
    }
}