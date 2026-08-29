namespace Shared.Financial.Response;

public class GetSearchCostCenterResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ManagerName { get; set; } = null!;
    public bool IsActive { get; set; }
}
