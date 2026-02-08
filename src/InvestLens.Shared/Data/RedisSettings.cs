using InvestLens.Abstraction.Redis.Data;

namespace InvestLens.Shared.Data;

public class RedisSettings : IRedisSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AllowAdmin { get; set; } = "false";
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 1000;
    public string Ssl { get; set; } = "false";
    public string ClientName { get; set; } = "InvestLensApp";
    public string InstanceName { get; set; } = string.Empty;
    public int DefaultDatabase { get; set; }

    // Полная строка подключения
    public string ConnectionString => $"{Host}:{Port},password={Password},user={Username}," +
                                      $"connectTimeout={ConnectTimeout},syncTimeout={SyncTimeout}," +
                                      $"ssl={Ssl}," +
                                      $"allowAdmin={AllowAdmin},name={ClientName}";
}