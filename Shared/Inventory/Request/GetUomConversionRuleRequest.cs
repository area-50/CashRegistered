namespace Shared.Inventory.Request;

public class GetUomConversionRuleRequest
{
    public int FromUomId { get; set; }
    
    public int ToUomId { get; set; }
    
    public int? ProductId { get; set; }
}
