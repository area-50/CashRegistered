using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Shared.Inventory.Response;

public class GetSearchCategoryResponse
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? ParentCategoryName { get; set; }

    public   int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }
}
