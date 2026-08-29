using Shared.Abstractions;

namespace Shared.Financial.Request;

public class SearchCostCenterRequest : PagedRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
