using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public PurchaseOrderItem(
        int orderId,
        int productId,
        int uomId,
        decimal orderedQuantity,
        decimal unitPrice,
        int? requisitionItemId = null
    )
    {
        OrderId = orderId;
        ProductId = productId;
        UomId = uomId;
        OrderedQuantity = orderedQuantity;
        UnitPrice = unitPrice;
        RequisitionItemId = requisitionItemId;
        ReceivedQuantity = 0;
    }

    protected PurchaseOrderItem() { }

    public int OrderId { get; set; }
    
    public PurchaseOrder Order { get; set; }
    
    public int ProductId { get; set; }
    
    public Product Product { get; set; }
    
    public int UomId { get; set; }
    
    public UnitOfMeasure Uom { get; set; }
    
    public decimal OrderedQuantity { get; set; }
    
    public decimal ReceivedQuantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public int? RequisitionItemId { get; set; }
    
    public PurchaseRequisitionItem? RequisitionItem { get; set; }
}
