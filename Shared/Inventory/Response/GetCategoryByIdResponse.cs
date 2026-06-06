namespace Shared.Inventory.Response;

public class GetCategoryByIdResponse
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }
}
