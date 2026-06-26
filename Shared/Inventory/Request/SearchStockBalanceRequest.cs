using Shared.Abstractions;

namespace Shared.Inventory.Request;

public class SearchStockBalanceRequest : PagedRequest
{
    public string? Term { get; set; }
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public bool? HideEmpty { get; set; }
}
