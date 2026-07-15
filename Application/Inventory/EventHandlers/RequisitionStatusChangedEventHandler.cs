using System.Text.Json;
using Domain.Shared.Interfaces;
using Domain.Inventory.Events;
using Domain.Shared.Events;
using Domain.Shared.Constants;
using Domain.Shared.Interfaces;
using Application.Inventory.Interfaces;

namespace Application.Inventory.EventHandlers;

public class RequisitionStatusChangedEventHandler : 
    INotificationHandler<RequisitionStatusChangedEvent>,
    INotificationHandler<ClientConnectedToTopicEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IInventoryRequisitionUseCase _useCase;

    public RequisitionStatusChangedEventHandler(INotificationService notificationService, IInventoryRequisitionUseCase useCase)
    {
        _notificationService = notificationService;
        _useCase = useCase;
    }

    public async Task Handle(RequisitionStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        await BroadcastCount();
    }

    public async Task Handle(ClientConnectedToTopicEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Topic == NotificationTopics.InventoryRequisitionsPending)
        {
            await BroadcastCount();
        }
    }

    private async Task BroadcastCount()
    {
        var pendingCount = await _useCase.GetPendingCountAsync();

        var payload = new
        {
            count = pendingCount,
            timestamp = DateTime.UtcNow
        };

        var jsonMessage = JsonSerializer.Serialize(payload);

        await _notificationService.PublishAsync(NotificationTopics.InventoryRequisitionsPending, jsonMessage);
    }
}
