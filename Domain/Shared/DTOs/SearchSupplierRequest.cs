using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchSupplierRequest : PagedRequest
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public bool? IsActive { get; set; }
}
