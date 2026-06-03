namespace Shared.Inventory.Request;

public class UpdateUnitOfMeasureRequest
{
    public string Code { get; set; }

    public string Name { get; set; }

    public bool AllowDecimals { get; set; }
    
    public bool IsActive { get; set; }
}