using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class PurchaseRequisitionItem : BaseEntity
{
    public PurchaseRequisitionItem(int requisitionId, int productId, int uomId, decimal requestedQuantity)
    {
        RequisitionId = requisitionId;
        ProductId = productId;
        UomId = uomId;
        RequestedQuantity = requestedQuantity;
    }

    protected PurchaseRequisitionItem() { }

    public int RequisitionId { get; set; }
    
    public PurchaseRequisition Requisition { get; set; }
    
    public int ProductId { get; set; }
    
    public Product Product { get; set; }
    
    public int UomId { get; set; }
    
    public UnitOfMeasure Uom { get; set; }
    
    public decimal RequestedQuantity { get; set; }
}
