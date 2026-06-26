namespace Shared.Inventory.Request;

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrincipal { get; set; }
}
