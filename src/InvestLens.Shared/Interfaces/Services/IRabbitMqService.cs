using InvestLens.Abstraction.MessageBus.Data;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace InvestLens.Shared.Interfaces.Services;

public interface IRabbitMqService
{
    Task EnsureRabbitMqIsRunningAsync(IConfiguration configuration, CancellationToken cancellation);
    Task<IConnection> GetConnection(IRabbitMqSettings settings, CancellationToken cancellationToken);
}