namespace Shared.Inventory.Response;

public class GetUnitOfMeasureByIdResponse
{
    public int Id { get; set; }
    
    public string Code { get; set; }

    public string Name { get; set; }

    public bool AllowDecimals { get; set; }
    
    public bool IsActive { get; set; }
}