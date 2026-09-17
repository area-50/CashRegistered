using Shared.Identity.Request;

namespace Shared.Identity.Request;

public class UpdateSupplierRequest
{
    public bool IsActive { get; set; }
    public UpdatePersonRequest? Person { get; set; }
}
