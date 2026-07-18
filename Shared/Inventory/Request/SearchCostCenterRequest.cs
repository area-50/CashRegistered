using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchCostCenterRequest : PagedRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
