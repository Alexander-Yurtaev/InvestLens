using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.Services;
using InvestLens.Shared.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace InvestLens.Shared.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly IPollyService _pollyService;
    private readonly ILogger<RabbitMqService> _logger;
    private static readonly SemaphoreSlim EnsureCheckLock = new(1, 1);

    public RabbitMqService(IPollyService pollyService, ILogger<RabbitMqService> logger)
    {
        _pollyService = pollyService;
        _logger = logger;
    }

    public async Task EnsureRabbitMqIsRunningAsync(IConfiguration configuration, CancellationToken cancellation)
    {
        HttpClient client = null!;
        await EnsureCheckLock.WaitAsync(cancellation);

        try
        {
            RabbitMqValidator.Validate(configuration);

            _logger.LogInformation("Waiting for RabbitMQ at {RabbitMqHost}...", configuration["RABBITMQ_HOST"]);

            var resilientPolicy = _pollyService.GetHttpResilientPolicy();
            client = new HttpClient();

            await resilientPolicy.ExecuteAsync(async () =>
            {
                var request = CreateHttpRequest(configuration);
                return await client.SendAsync(request, cancellation);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{ex.Message}");
            throw;
        }
        finally
        {
            EnsureCheckLock.Release();
            client.Dispose();
        }
    }

    public async Task<IConnection> GetConnection(IRabbitMqSettings settings, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true, // Автовосстановление
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            ContinuationTimeout = TimeSpan.FromSeconds(20),
            HandshakeContinuationTimeout = TimeSpan.FromSeconds(20)
        };

        if (!string.IsNullOrEmpty(settings.ClientName))
        {
            factory.ClientProvidedName = settings.ClientName;
        }

        var resilientPolicy = _pollyService.GetResilientPolicy<BrokerUnreachableException>();
        return await resilientPolicy.ExecuteAsync(async ()=> await factory.CreateConnectionAsync(cancellationToken));
    }

    #region Private Methods

    private static HttpRequestMessage CreateHttpRequest(IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RABBITMQ_HOST"];
        var username = configuration["RABBITMQ_USER"];
        var password = configuration["RABBITMQ_PASSWORD"];

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"http://{rabbitMqHost}:15672/api/healthchecks/node");
        var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

        return request;
    }

    #endregion Private Methods
}