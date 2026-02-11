using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Constants;
using InvestLens.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InvestLens.Shared.Interfaces.MessageBus.Models;
using InvestLens.Shared.Interfaces.Services;

namespace InvestLens.Shared.Services.RabbitMq;

public class RabbitMqClient : IMessageBusClient
{
    private IConnection _connection = null!;
    private IChannel _channel = null!;
    private readonly IRabbitMqSettings _settings;
    private readonly IRabbitMqService _service;
    private readonly ILogger<RabbitMqClient> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, AsyncEventingBasicConsumer> _consumers;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _publishLock = new();

    public static async Task<RabbitMqClient> CreateAsync(
        IRabbitMqSettings settings,
        IRabbitMqService service,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqClient> logger)
    {
        var client = new RabbitMqClient(settings, service, serviceProvider, logger);
        await client.InitializeAsync();
        return client;
    }

    internal RabbitMqClient(
        IRabbitMqSettings settings,
        IRabbitMqService service,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqClient> logger)
    {
        _settings = settings;
        _service = service;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumers = new ConcurrentDictionary<string, AsyncEventingBasicConsumer>();

        ValidateSettings(_settings);

        // Настройка JSON сериализации
        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        _logger.LogInformation("The RabbitMQ Client has been created");
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _connection = await _service.GetConnection(_settings, cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Настройка качества обслуживания (QoS)
        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _settings.PrefetchCount,
            global: false, cancellationToken: cancellationToken);

        _logger.LogInformation("The RabbitMQ Client is connected to {Host}", _settings.HostName);
    }

    public async Task<IChannel> GetChannelAsync()
    {
        return await Task.FromResult(_channel);
    }

    public async Task PublishAsync<T>(
        T message,
        string exchangeName,
        string routingKey = "",
        CancellationToken cancellationToken = default) where T : IBaseMessage
    {
        try
        {
            lock (_publishLock)
            {
                if (_channel.IsClosed)
                {
                    throw new InvalidOperationException("The channel is closed");
                }
            }

            // Объявляем exchange если его нет
            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            // В версии 7.2.0 создаем BasicProperties через Channel
            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = message.MessageId.ToString(),
                Type = typeof(T).Name,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    ["x-app-name"] = _settings.ClientName,
                    ["x-origin"] = Environment.MachineName
                }
            };

            foreach (var header in message.Headers)
            {
                properties.Headers[header.Key] = header.Value.ToString();
            }

            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("The {messageId} message is published in {Exchange}/{routingKey}",
                message.MessageId, exchangeName, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when posting a message {messageId}", message.MessageId);
            throw new MessageBusException($"Failed to publish message {message.MessageId}", ex);
        }
    }

    public virtual async Task SubscribeAsync<T, TH>(
        string queueName,
        string exchangeName = "",
        string routingKey = "",
        CancellationToken cancellationToken = default)
        where T : IBaseMessage
        where TH : IMessageHandler<T>
    {
        try
        {
            if (_consumers.ContainsKey(queueName))
            {
                _logger.LogWarning("The consumer is already registered for the queue {QueueName}", queueName);
                return;
            }

            if (!string.IsNullOrEmpty(exchangeName))
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                // Настройка DLX для очереди
                var arguments = new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = _settings.DeadLetterExchange ?? "dlx-exchange",
                    ["x-dead-letter-routing-key"] = queueName + ".dlq"
                };

                // Создаем очередь с настройками
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: arguments!,
                    cancellationToken: cancellationToken);

                // Привязываем очередь к exchange
                await _channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey,
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Если exchange не указан, работаем напрямую с очередью
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            // Регистрируем обработчики событий
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var correlationId = ea.BasicProperties.Headers?[HeaderConstants.CorrelationHeader] as string;
                using (LogContext.PushProperty("CorrelationId", correlationId))
                {
                    await OnMessageReceived<T, TH>(ea, cancellationToken);
                }
            };

            consumer.ShutdownAsync += OnConsumerShutdown;
            consumer.RegisteredAsync += OnConsumerRegistered;
            consumer.UnregisteredAsync += OnConsumerUnregistered;

            // Начинаем потребление
            var consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _consumers[queueName] = consumer;

            _logger.LogInformation(
                "The consumer {ConsumerTag} is registered for the queue {QueueName}",
                consumerTag, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when creating a subscription to the queue {QueueName}", queueName);
            throw new MessageBusException($"Failed to subscribe to queue {queueName}", ex);
        }
    }

    private async Task OnMessageReceived<T, TH>(
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
        where T : IBaseMessage
        where TH : IMessageHandler<T>
    {
        var messageId = ea.BasicProperties.MessageId;
        var deliveryTag = ea.DeliveryTag;

        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            if (message == null)
            {
                _logger.LogError("Couldn't deserialize message {messageId}", messageId);
                await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                return;
            }

            _logger.LogDebug("{messageId} message received from {Queue}",
                messageId, ea.RoutingKey);

            // Создаем обработчик через DI
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetService<TH>();

            if (handler != null)
            {
                var success = await handler.HandleAsync(message, cancellationToken);

                _ = Task.Run(async () =>
                {
                    if (success)
                    {
                        await _channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                        _logger.LogDebug("Message {messageId} successfully processed", messageId);
                    }
                    else
                    {
                        // Отправляем в DLQ после неудачной обработки
                        await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                        _logger.LogWarning("The handler returned false for the {messageId} message", messageId);
                    }
                }, cancellationToken);
            }
            else
            {
                _logger.LogError("No handler found for the {Type} type", typeof(TH).Name);
                await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the message {messageId}", messageId);

            // Проверяем количество повторных попыток
            var redeliveryCount = GetRedeliveryCount(ea.BasicProperties);

            if (redeliveryCount < (_settings.MaxRedeliveryCount ?? 3))
            {
                // Возвращаем в очередь для повторной попытки
                await _channel.BasicNackAsync(deliveryTag, false, true, cancellationToken);

                var delay = TimeSpan.FromSeconds(Math.Pow(2, redeliveryCount)); // Exponential backoff
                await Task.Delay(delay, cancellationToken);
            }
            else
            {
                // Отправляем в DLQ
                await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                _logger.LogWarning("{messageId} message sent to DLQ after {Count} attempts",
                    messageId, redeliveryCount);
            }
        }
    }

    private static int GetRedeliveryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers != null &&
            properties.Headers.TryGetValue("x-redelivery-count", out var value) &&
            value is long count)
        {
            return (int)count;
        }

        return 0;
    }

    private Task OnConsumerShutdown(object sender, ShutdownEventArgs args)
    {
        _logger.LogInformation(
            "The consumer is stopped. Reason: {ReplyText}, Initiator: {Initiator}",
            args.ReplyText, args.Initiator);
        return Task.CompletedTask;
    }

    private Task OnConsumerRegistered(object sender, ConsumerEventArgs args)
    {
        _logger.LogDebug("The consumer is registered: {ConsumerTags}", string.Join(',', args.ConsumerTags));
        return Task.CompletedTask;
    }

    private Task OnConsumerUnregistered(object sender, ConsumerEventArgs args)
    {
        _logger.LogDebug("Потребитель отменен: {ConsumerTag}", string.Join(',', args.ConsumerTags));
        return Task.CompletedTask;
    }

    public async Task UnsubscribeAsync<T, TH>(CancellationToken cancellationToken = default)
        where T : IBaseMessage
        where TH : IMessageHandler<T>
    {
        var queueName = _consumers.Keys.FirstOrDefault(k => k.Contains(typeof(T).Name));

        if (queueName != null && _consumers.TryRemove(queueName, out var consumer))
        {
            var consumerTag = consumer.ConsumerTags.FirstOrDefault();

            if (consumerTag != null)
            {
                await _channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
                _logger.LogInformation("Unsubscribed from the queue: {QueueName}", queueName);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var consumer in _consumers.Values)
        {
            foreach (var tag in consumer.ConsumerTags)
            {
                await _channel.BasicCancelAsync(tag);
            }
        }

        _consumers.Clear();

        if (_channel.IsOpen)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection.IsOpen)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _logger.LogInformation("The RabbitMQ Client is disabled");

        // Предотвращаем вызов финализатора для этого объекта
        GC.SuppressFinalize(this);
    }

    private static void ValidateSettings(IRabbitMqSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        if (string.IsNullOrWhiteSpace(settings.HostName))
            throw new ArgumentException("The HostName cannot be empty", nameof(settings));

        if (settings.Port <= 0)
            throw new ArgumentException("The port must be a positive number.", nameof(settings));

        if (settings.PrefetchCount <= 0)
            throw new ArgumentException("prefetchCount must be a positive number.", nameof(settings));
    }
}