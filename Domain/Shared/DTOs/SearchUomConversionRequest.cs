using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchUomConversionRequest : PagedRequest
{
    public string? Term { get; set; }
}