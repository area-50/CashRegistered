namespace Shared.Inventory.Request;

public class CreateInventoryRequisitionRequest
{
    public string OriginModule { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string? Notes { get; set; }
    public List<CreateInventoryRequisitionItemRequest> Items { get; set; } = [];
}

public class CreateInventoryRequisitionItemRequest
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}
