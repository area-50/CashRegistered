using Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetMeResponse
{
    public string UserName { get; set; } = string.Empty;
    public Name Name { get; set; } 
}
