using Domain.Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetAllUsersResponse
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    
    public Name? Name { get; set; }

    public DateTime Birthdate { get; set; }

    public string? TaxId { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}