using Domain.Inventory.Enums;
using Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class InventoryRequisition : BaseEntity
{
    public string OriginModule { get; private set; }
    
    public int RequestedByUserId { get; private set; }
    
    public RequisitionStatus Status { get; private set; }
    
    public DateTime? FulfilledAt { get; private set; }
    
    public string? Notes { get; private set; }

    private readonly List<InventoryRequisitionItem> _items = [];
    
    public IReadOnlyCollection<InventoryRequisitionItem> Items => _items.AsReadOnly();

    public InventoryRequisition(string originModule, int requestedByUserId, string? notes = null)
    {
        OriginModule = originModule;
        RequestedByUserId = requestedByUserId;
        Status = RequisitionStatus.Pending;
        Notes = notes;
    }

    public void AddItem(int productId, decimal quantity)
    {
        if (quantity <= 0)
        {
            AddNotification("Quantity", "A quantidade deve ser maior que zero.");
            return;
        }

        _items.Add(new InventoryRequisitionItem(productId, quantity));
    }

    public void Fulfill()
    {
        if (Status != RequisitionStatus.Pending)
        {
            AddNotification("Status", "Apenas requisições pendentes podem ser baixadas.");
            return;
        }

        Status = RequisitionStatus.Fulfilled;
        FulfilledAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != RequisitionStatus.Pending)
        {
            AddNotification("Status", "Apenas requisições pendentes podem ser canceladas.");
            return;
        }

        Status = RequisitionStatus.Canceled;
    }

    // Required for EF
    protected InventoryRequisition() { }
}
