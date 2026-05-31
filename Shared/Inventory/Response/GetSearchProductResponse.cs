namespace Shared.Inventory.Response;

public class GetSearchProductResponse
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Sku { get; set; }
    
    public string Category { get; set; }
    
    public string UomSymbol { get; set; }
    
    public bool IsActive { get; set; }

}