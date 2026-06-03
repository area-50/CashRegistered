namespace Shared.Inventory.Request;

public class UpdateProductRequest : CreateProductRequest
{
    public bool IsActive { get; set; }
}