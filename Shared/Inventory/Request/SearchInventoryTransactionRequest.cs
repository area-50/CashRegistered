using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchInventoryTransactionRequest : PagedRequest
{
    public string? ReferenceDocument { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
