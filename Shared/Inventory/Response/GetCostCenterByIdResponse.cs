namespace Shared.Inventory.Response;

public class GetCostCenterByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = null!;
    public bool IsActive { get; set; }
}
