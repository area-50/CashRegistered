namespace Shared.Inventory.Response;

public class GetInventoryRequisitionByIdResponse
{
    public int Id { get; set; }
    public string OriginModule { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public string? Notes { get; set; }
    
    public List<GetInventoryRequisitionItemResponse> Items { get; set; } = [];
}

public class GetInventoryRequisitionItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
