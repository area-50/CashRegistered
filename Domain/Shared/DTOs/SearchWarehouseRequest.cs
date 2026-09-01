using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchWarehouseRequest : PagedRequest
{
    public string? Term { get; set; }
}
