namespace Shared.Inventory.Response;

public class GetWarehouseByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPrincipal { get; set; }
}
