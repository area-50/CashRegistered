using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchInventoryTransactionRequest : PagedRequest
{
    public string? ReferenceDocument { get; set; }
}
