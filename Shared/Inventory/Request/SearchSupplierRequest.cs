using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchSupplierRequest : PagedRequest
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
}
