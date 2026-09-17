namespace Shared.Identity.Request;

public class AdminUpdateUserRequest
{
    public string UserName { get; set; } = null!;
    
    public string Role { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public DateTime Birthdate { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string? CellPhone { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Gender { get; set; }
}
