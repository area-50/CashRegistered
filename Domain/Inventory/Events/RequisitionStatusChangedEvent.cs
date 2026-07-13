using Domain.Shared.Events;

namespace Domain.Inventory.Events;

public class RequisitionStatusChangedEvent : DomainEvent
{
    public int RequisitionId { get; }
    public string NewStatus { get; }

    public RequisitionStatusChangedEvent(int requisitionId, string newStatus)
    {
        RequisitionId = requisitionId;
        NewStatus = newStatus;
    }
}
