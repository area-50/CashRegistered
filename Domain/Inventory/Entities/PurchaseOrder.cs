using Domain.Inventory.Enums;
using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class PurchaseOrder : BaseEntity
{
    public PurchaseOrder(int supplierId)
    {
        IssueDate = DateTime.UtcNow;
        SupplierId = supplierId;
        Status = PurchaseOrderStatus.Issued;
    }

    protected PurchaseOrder() { }

    public int SupplierId { get; set; }
    
    public Supplier Supplier { get; set; }
    
    public DateTime IssueDate { get; set; }
    
    public PurchaseOrderStatus Status { get; set; }
    
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
