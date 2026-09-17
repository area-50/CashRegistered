using Domain.Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetUserByIdResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Name? Name { get; set; }
    public DateTime Birthdate { get; set; }
    public string? TaxId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CellPhone { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
}
