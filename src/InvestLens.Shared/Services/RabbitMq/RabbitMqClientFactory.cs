using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Shared.Interfaces.MessageBus.Services;
using InvestLens.Shared.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.Services.RabbitMq;

public class RabbitMqClientFactory : IRabbitMqClientFactory
{
    private readonly IRabbitMqSettings _settings;
    private readonly IRabbitMqService _service;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqClient> _logger;

    public RabbitMqClientFactory(
        IRabbitMqSettings settings,
        IRabbitMqService service,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqClient> logger)
    {
        _settings = settings;
        _service = service;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IMessageBusClient> CreateRabbitMqClient(CancellationToken cancellationToken)
    {
        var client = await RabbitMqClient.CreateAsync(_settings, _service, _serviceProvider, _logger);
        return client;
    }
}