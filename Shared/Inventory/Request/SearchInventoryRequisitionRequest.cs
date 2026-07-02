using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchInventoryRequisitionRequest : PagedRequest
{
    public string? OriginModule { get; set; }
    
    public int? Status { get; set; } // 1=Pending, 2=Fulfilled, 3=Canceled
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
