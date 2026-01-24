using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using System.Net.Sockets;

namespace InvestLens.Shared.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private static readonly SemaphoreSlim EnsureCheckLock = new(1, 1);

    // Кэшированные политики для производительности
    private readonly AsyncPolicy _tcpHealthCheckPolicy; // Политика для TCP проверки
    private readonly AsyncPolicy _rabbitMqResilientPolicy;

    public RabbitMqService(IPollyService pollyService, ILogger<RabbitMqService> logger)
    {
        _logger = logger;

        // Инициализируем политики один раз
        _rabbitMqResilientPolicy = pollyService.GetRabbitMqResilientPolicy();

        // Создаем политику для TCP health check (больше попыток, больше времени)
        _tcpHealthCheckPolicy = Policy
            .Handle<SocketException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 30, // Увеличиваем до 30 попыток (~2 минуты)
                sleepDurationProvider: _ => TimeSpan.FromSeconds(2), // Фиксированная задержка 2 секунды
                onRetry: (exception, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                        "RabbitMQ TCP check retry {RetryAttempt}/30 after {Seconds}s: {Message}",
                        retryAttempt, timespan.TotalSeconds, exception.Message);
                });
    }

    public async Task EnsureRabbitMqIsRunningAsync(IConfiguration configuration, CancellationToken cancellation)
    {
        await EnsureCheckLock.WaitAsync(cancellation);

        try
        {
            RabbitMqValidator.Validate(configuration);

            var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "localhost";
            var rabbitMqPort = int.Parse(configuration["RABBITMQ_PORT"] ?? "5672");

            _logger.LogInformation(
                "Waiting for RabbitMQ at {RabbitMqHost}:{RabbitMqPort}...",
                rabbitMqHost, rabbitMqPort);

            // Используем TCP-ориентированную политику для проверки health check
            await _tcpHealthCheckPolicy.ExecuteAsync(async (ct) =>
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(rabbitMqHost, rabbitMqPort, ct);

                _logger.LogInformation(
                    "RabbitMQ TCP health check successful at {RabbitMqHost}:{RabbitMqPort}",
                    rabbitMqHost, rabbitMqPort);
            }, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed: {Message}", ex.Message);
            throw;
        }
        finally
        {
            EnsureCheckLock.Release();
        }
    }

    public async Task<IConnection> GetConnection(IRabbitMqSettings settings, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Creating RabbitMQ connection to {Host}:{Port}",
            settings.HostName, settings.Port);

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            ContinuationTimeout = TimeSpan.FromSeconds(20),
            HandshakeContinuationTimeout = TimeSpan.FromSeconds(20)
        };

        if (!string.IsNullOrEmpty(settings.ClientName))
        {
            factory.ClientProvidedName = settings.ClientName;
        }

        // Используем полноценную resilient политику (Retry + Circuit Breaker)
        return await _rabbitMqResilientPolicy.ExecuteAsync(async (ct) =>
        {
            try
            {
                var connection = await factory.CreateConnectionAsync(ct);
                _logger.LogInformation(
                    "RabbitMQ connection established to {Host}:{Port}",
                    settings.HostName, settings.Port);

                // Настройка обработчиков событий соединения
                connection.ConnectionShutdownAsync += async (_, args) =>
                {
                    _logger.LogWarning(
                        "RabbitMQ connection shutdown: {ReplyText}",
                        args.ReplyText);
                    await Task.CompletedTask;
                };

                connection.ConnectionBlockedAsync += async (_, args) =>
                {
                    _logger.LogWarning(
                        "RabbitMQ connection blocked: {Reason}",
                        args.Reason);
                    await Task.CompletedTask;
                };

                connection.ConnectionUnblockedAsync += async (_, _) =>
                {
                    _logger.LogInformation("RabbitMQ connection unblocked");
                    await Task.CompletedTask;
                };

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create RabbitMQ connection to {Host}:{Port}",
                    settings.HostName, settings.Port);
                throw;
            }
        }, cancellationToken);
    }
}