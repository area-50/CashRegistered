namespace Domain.Shared.Interfaces;

public interface IEventDispatcher
{
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : INotification;
}
