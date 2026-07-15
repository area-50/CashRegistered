using Domain.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : INotification
    {
        // Resolve todos os Handlers nativos registrados via Injeção de Dependência
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        
        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken);
        }
    }
}
