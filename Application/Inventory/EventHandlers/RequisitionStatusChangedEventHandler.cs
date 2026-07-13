using System.Text.Json;
using Domain.Shared.Interfaces;
using Domain.Inventory.Events;
using MediatR;
using Application.Inventory.Interfaces;

namespace Application.Inventory.EventHandlers;

public class RequisitionStatusChangedEventHandler : INotificationHandler<RequisitionStatusChangedEvent>
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
        var pendingCount = await _useCase.GetPendingCountAsync();

        var payload = new
        {
            count = pendingCount,
            timestamp = DateTime.UtcNow
        };

        var jsonMessage = JsonSerializer.Serialize(payload);

        await _notificationService.PublishAsync("inventory.requisitions.pending", jsonMessage);
    }
}
