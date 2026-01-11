using InvestLens.Abstraction.MessageBus.Models;

namespace InvestLens.Shared.MessageBus.Models;

public abstract class BaseMessage : IBaseMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = string.Empty;
    public Dictionary<string, object> Headers { get; set; } = new();
}