using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchCostCenterRequest : PagedRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
