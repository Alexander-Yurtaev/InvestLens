using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Shared.Interfaces.Services;
using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using System.Net.Sockets;

namespace InvestLens.Shared.Services.RabbitMq;

public class RabbitMqService : IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private static readonly SemaphoreSlim EnsureCheckLock = new(1, 1);

    // Кэшированные политики для производительности
    private readonly AsyncPolicy _rabbitMqResilientPolicy;
    private IConnection? _connection = null;

    public RabbitMqService(IPollyService pollyService, ILogger<RabbitMqService> logger)
    {
        _logger = logger;

        // Инициализируем политики один раз
        _rabbitMqResilientPolicy = pollyService.GetRabbitMqResilientPolicy();
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
            await _rabbitMqResilientPolicy.ExecuteAsync(async (ct) =>
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
        if (_connection is null || !_connection.IsOpen)
        {
            _connection = await CreateConnection(settings, cancellationToken);
        }

        return _connection;
    }

    #region Private Methods

    private async Task<IConnection> CreateConnection(IRabbitMqSettings settings, CancellationToken cancellationToken)
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

            RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
            ContinuationTimeout = TimeSpan.FromSeconds(5),
            HandshakeContinuationTimeout = TimeSpan.FromSeconds(5),

            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60)
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

    #endregion Private Methods
}