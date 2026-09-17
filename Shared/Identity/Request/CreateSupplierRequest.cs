using Shared.Identity.Request;

namespace Shared.Identity.Request;

public class CreateSupplierRequest
{
    public int? PersonId { get; set; }
    public CreatePersonRequest? Person { get; set; }
}
