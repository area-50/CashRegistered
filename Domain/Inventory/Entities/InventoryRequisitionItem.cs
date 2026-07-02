using Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class InventoryRequisitionItem : BaseEntity
{
    public int InventoryRequisitionId { get; private set; }
    public int ProductId { get; private set; }
    public decimal Quantity { get; private set; }

    // Navigations
    public InventoryRequisition Requisition { get; private set; }
    public Product Product { get; private set; }

    public InventoryRequisitionItem(int productId, decimal quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    // Required for EF
    protected InventoryRequisitionItem() { }
}
