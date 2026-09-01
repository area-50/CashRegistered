using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchCategoryRequest : PagedRequest
{
    public string? Term { get; set; }
}