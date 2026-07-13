using MediatR;

namespace Domain.Shared.Events;

public abstract class DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
