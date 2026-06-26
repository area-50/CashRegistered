namespace Shared.Inventory.Request;

public class CreateInventoryTransactionRequest
{
    public int UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<CreateInventoryTransactionItemRequest> Items { get; set; } = [];
}

public class CreateInventoryTransactionItemRequest
{
    public int ProductId { get; set; }
    public int UomId { get; set; }
    public decimal TransactionQuantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
}
