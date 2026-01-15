using InvestLens.Abstraction.MessageBus.Data;
using InvestLens.Abstraction.MessageBus.Services;
using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Logging;

namespace InvestLens.Shared.MessageBus.Services;

public class RabbitMqClientFactory : IRabbitMqClientFactory
{
    private readonly IRabbitMqSettings _settings;
    private readonly IRabbitMqService _service;
    private readonly ILogger<RabbitMqClient> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqClientFactory(
        IRabbitMqSettings settings,
        IRabbitMqService service,
        ILogger<RabbitMqClient> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _service = service;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<IMessageBusClient> CreateRabbitMqClient(CancellationToken cancellationToken)
    {
        var client = await RabbitMqClient.CreateAsync(_settings, _service, _logger, _serviceProvider);
        return client;
    }
}