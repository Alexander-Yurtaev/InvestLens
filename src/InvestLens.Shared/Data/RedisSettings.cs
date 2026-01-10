using System.Text.Json.Serialization;
using InvestLens.Abstraction.Data;

namespace InvestLens.Shared.Data;

public record RedisSettings : IRedisSettings
{
    public int DefaultDatabase { get; init; } = 0;
    public string InstanceName { get; init; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string Host { get; set; } = string.Empty;
    
    public int Timeout { get; init; } = 0;
    
    public int Ssl { get; init; } = 0;
    
    public int AllowAdmin { get; init; } = 0;
    
    public string ConnectionString => $"redis://{Username}:{Password}@{Host}:6379/0?connectTimeout={Timeout}&ssl={Ssl}&allowAdmin={AllowAdmin}";
}