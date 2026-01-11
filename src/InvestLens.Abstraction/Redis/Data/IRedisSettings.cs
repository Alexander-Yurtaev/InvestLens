namespace InvestLens.Abstraction.Redis.Data;

public interface IRedisSettings
{
    string Host { get; set; }
    int Port { get; set; }
    string Password { get; set; }
    string Username { get; set; }
    string AllowAdmin { get; set; }
    int ConnectTimeout { get; set; }
    int SyncTimeout { get; set; }
    string Ssl { get; set; }
    string ClientName { get; set; }
    string InstanceName { get; set; }
    int DefaultDatabase { get; set; }
    string ConnectionString { get; }
}