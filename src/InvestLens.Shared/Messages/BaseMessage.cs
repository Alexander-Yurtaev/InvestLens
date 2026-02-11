using InvestLens.Shared.Interfaces.MessageBus.Models;

namespace InvestLens.Shared.Messages;

public abstract class BaseMessage : IBaseMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public Dictionary<string, object> Headers { get; set; } = new();
}