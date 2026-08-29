using Domain.Inventory.Enums;
using Domain.Financial.Entities;
using Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class InternalRequisition : BaseEntity
{
    public InternalRequisition(int costCenterId)
    {
        CostCenterId = costCenterId;
        RequestDate = DateTime.UtcNow;
        Status = InternalRequisitionStatus.Pending;
    }

    protected InternalRequisition() { }

    public int CostCenterId { get; set; }
    
    public CostCenter CostCenter { get; set; }
    
    public DateTime RequestDate { get; set; }
    
    public InternalRequisitionStatus Status { get; set; }
    
    public ICollection<InternalRequisitionItem> Items { get; set; } = new List<InternalRequisitionItem>();
}
