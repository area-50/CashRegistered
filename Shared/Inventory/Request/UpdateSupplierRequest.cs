namespace Shared.Inventory.Request;

public class UpdateSupplierRequest
{
    public bool IsActive { get; set; }
    public Shared.Identity.Request.UpdatePersonRequest? Person { get; set; }
}
