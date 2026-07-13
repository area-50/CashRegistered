using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchInventoryRequisitionRequest : PagedRequest
{
    public string? OriginModule { get; set; }
    
    public string? Status { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
