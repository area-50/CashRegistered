using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchInventoryTransactionRequest : PagedRequest
{
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TransactionType { get; set; }
    public string? TransactionStatus { get; set; }
}
