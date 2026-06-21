using Shared.ValueObjects;

namespace Shared.Security.Response;

public class LoginUserResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    public int Id { get; set; }

    public Name? UserName { get; set; }

    public string Role { get; set; } = string.Empty;

}