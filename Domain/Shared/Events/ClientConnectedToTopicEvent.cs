using Domain.Shared.Events;

namespace Domain.Shared.Events;

public class ClientConnectedToTopicEvent : DomainEvent
{
    public string Topic { get; }

    public ClientConnectedToTopicEvent(string topic)
    {
        Topic = topic;
    }
}
