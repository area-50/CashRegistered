namespace Shared.Inventory.Request;

public class UpdateUomConversionRequest : CreateUomConversionRequest
{
    public bool IsActive { get; set; }
}
