using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchCustomerRequest : PagedRequest
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public bool? IsActive { get; set; }
}
