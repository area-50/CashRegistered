using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchProductRequest : PagedRequest
{
    public string? Term { get; set; }
    
    public int? CategoryId { get; set; }
}