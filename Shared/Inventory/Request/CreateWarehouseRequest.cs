namespace Shared.Inventory.Request;

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
}
