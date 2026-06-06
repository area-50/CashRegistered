namespace Shared.Inventory.Request;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }
}
