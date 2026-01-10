namespace InvestLens.Abstraction.Data;

public interface IRedisSettings
{
    int DefaultDatabase { get; init; }
    string InstanceName { get; init; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Host { get; set; }

    public int Timeout { get; init; }

    public int Ssl { get; init; }

    public int AllowAdmin { get; init; }

    string ConnectionString { get; }
}