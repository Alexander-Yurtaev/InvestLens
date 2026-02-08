using InvestLens.Abstraction.MessageBus.Data;

namespace InvestLens.Shared.Data;

public record RabbitMqSettings : IRabbitMqSettings
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; init; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ClientName { get; init; } = "Invest Lens";
    public ushort PrefetchCount { get; set; } = 10;
    public int? MaxRedeliveryCount { get; set; } = 3;
    public string? DeadLetterExchange { get; set; } = "dlx-exchange";
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
}