namespace Shared.Inventory.Response;

public class GetUomConversionRuleResponse
{
    public int Id { get; set; }
    
    public decimal Multiplier { get; set; }
    
    public bool IsActive { get; set; }
}
