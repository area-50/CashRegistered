using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchUserRequest : PagedRequest
{
    public string? Name { get; set; }
    public string? TaxId { get; set; }
    public DateTime? BirthDate { get; set; }
}
