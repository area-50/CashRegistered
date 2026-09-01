using Domain.Inventory.Enums;
using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class PurchaseRequisition : BaseEntity
{
    public PurchaseRequisition(int userId)
    {
        Date = DateTime.UtcNow;
        UserId = userId;
        Status = PurchaseRequisitionStatus.Open;
    }

    protected PurchaseRequisition() { }

    public DateTime Date { get; set; }
    
    public int UserId { get; set; }
    
    public User User { get; set; }
    
    public PurchaseRequisitionStatus Status { get; set; }
    
    public ICollection<PurchaseRequisitionItem> Items { get; set; } = new List<PurchaseRequisitionItem>();
}
