using Shared.Identity.Request;

namespace Shared.Inventory.Request;

public class CreateSupplierRequest
{
    public int? PersonId { get; set; }
    
    public CreatePersonRequest? Person { get; set; }
}
