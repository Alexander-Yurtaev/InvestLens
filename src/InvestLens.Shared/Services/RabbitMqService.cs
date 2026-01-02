using InvestLens.Abstraction.Services;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Shared.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConfiguration _configuration;

    public RabbitMqService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #region Privaet Methods

    private string GetConnectionString()
    {
        var radditmq_host = _configuration["RABBITMQ_HOST"];
        var username = _configuration["RABBITMQ_USER"];
        var password = _configuration["RABBITMQ_PASSWORD"];
        var radditmq_vhost = _configuration["RABBITMQ_VHOST"];

        // amqp://user:password@localhost:5672/my_vhost
        return $"amqp://{username}:{password}@{radditmq_host}:5672/{radditmq_vhost}";
    }

    #endregion Privaet Methods
}
