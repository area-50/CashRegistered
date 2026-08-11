namespace Shared.Inventory.Response;

public class ProductConversionItemResponse
{
    public int UomId { get; set; }
    public string UomSymbol { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public string RuleType { get; set; } = string.Empty;
}
