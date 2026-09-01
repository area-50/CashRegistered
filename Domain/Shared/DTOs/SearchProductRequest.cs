using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchProductRequest : PagedRequest
{
    public string? Term { get; set; }
    
    public int? CategoryId { get; set; }

    public int? WarehouseId { get; set; }
}