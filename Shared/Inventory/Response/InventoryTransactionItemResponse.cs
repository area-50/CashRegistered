namespace Shared.Inventory.Response;

public class InventoryTransactionItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int? SourceWarehouseId { get; set; }
    public string? SourceWarehouseName { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public string? DestinationWarehouseName { get; set; }
}
