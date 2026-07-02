namespace Shared.Inventory.Response;

public class SearchInventoryRequisitionResponse
{
    public int Id { get; set; }
    public string OriginModule { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public int Status { get; set; } // 1=Pending, 2=Fulfilled, 3=Canceled
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public bool IsActive { get; set; }
}
