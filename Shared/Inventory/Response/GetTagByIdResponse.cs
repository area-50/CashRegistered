namespace Shared.Inventory.Response;

public class GetTagByIdResponse
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string? ColorHex { get; set; }
    
    public bool IsActive { get; set; }
}