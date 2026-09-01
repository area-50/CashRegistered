using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchUnitOfMeasureRequest : PagedRequest
{
    public string? Term { get; set; }
}
