using System.Text.Json;
using Domain.Shared.Interfaces;
using Domain.Inventory.Events;
using Domain.Shared.Events;
using Domain.Shared.Constants;
using Application.Inventory.Interfaces;

namespace Application.Inventory.EventHandlers;

public class RequisitionStatusChangedEventHandler(
    INotificationService notificationService,
    IInventoryRequisitionUseCase useCase)
    :
        INotificationHandler<RequisitionStatusChangedEvent>,
        INotificationHandler<ClientConnectedToTopicEvent>
{
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
        var pendingCount = await useCase.GetPendingCountAsync();

        var payload = new
        {
            count = pendingCount,
            timestamp = DateTime.UtcNow
        };

        var jsonMessage = JsonSerializer.Serialize(payload);

        await notificationService.PublishAsync(NotificationTopics.InventoryRequisitionsPending, jsonMessage);
    }
}
