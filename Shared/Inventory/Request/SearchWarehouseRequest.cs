using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchWarehouseRequest : PagedRequest
{
    public string? Term { get; set; }
}
