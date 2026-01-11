namespace InvestLens.Abstraction.MessageBus.Data;

public interface IRabbitMqSettings
{
    string HostName { get; set; }
    int Port { get; init; }
    string UserName { get; set; }
    string Password { get; set; }
    string VirtualHost { get; set; }
    string ClientName { get; init; }
    ushort PrefetchCount { get; set; }
    int? MaxRedeliveryCount { get; set; }
    string? DeadLetterExchange { get; set; }
    TimeSpan ConnectionTimeout { get; set; }
}