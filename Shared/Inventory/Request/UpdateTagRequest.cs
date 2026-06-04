namespace Shared.Inventory.Request;

public class UpdateTagRequest
{
    public string Name { get; set; }
    
    public string? ColorHex { get; set; }

    public bool IsActive { get; set; }
}