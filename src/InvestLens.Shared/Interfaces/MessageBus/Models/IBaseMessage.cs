namespace InvestLens.Abstraction.MessageBus.Models;

public interface IBaseMessage
{
    Guid MessageId { get; set; }
    DateTime CreatedAt { get; set; }
    string MessageType { get; set; }
    Dictionary<string, object> Headers { get; set; }
}