using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class InternalRequisitionItem : BaseEntity
{
    public InternalRequisitionItem(
        int internalRequisitionId,
        int productId,
        int uomId,
        decimal requestedQuantity
    )
    {
        InternalRequisitionId = internalRequisitionId;
        ProductId = productId;
        UomId = uomId;
        RequestedQuantity = requestedQuantity;
        FulfilledQuantity = 0;
    }

    protected InternalRequisitionItem() { }

    public int InternalRequisitionId { get; set; }
    
    public InternalRequisition InternalRequisition { get; set; }
    
    public int ProductId { get; set; }
    
    public Product Product { get; set; }
    
    public int UomId { get; set; }
    
    public UnitOfMeasure Uom { get; set; }
    public decimal RequestedQuantity { get; set; }
    
    public decimal FulfilledQuantity { get; set; }
}
