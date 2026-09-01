namespace Shared.Identity.Request;

public class CreateUserPayload
{
    public CreateUserRequest UserRequest { get; set; } = null!;
    public CreatePersonRequest? PersonRequest { get; set; }
}
