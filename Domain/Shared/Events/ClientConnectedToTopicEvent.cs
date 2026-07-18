namespace Domain.Shared.Events;

public class ClientConnectedToTopicEvent(string topic) : DomainEvent
{
    public string Topic { get; } = topic;
}
