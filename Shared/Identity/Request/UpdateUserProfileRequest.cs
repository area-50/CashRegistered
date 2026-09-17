namespace Shared.Identity.Request;

public class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public DateTime Birthdate { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string? CellPhone { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Gender { get; set; }
}
