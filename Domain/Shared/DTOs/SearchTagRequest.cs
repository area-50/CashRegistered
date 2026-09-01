using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchTagRequest : PagedRequest
{
    public string? Term { get; set; }
}