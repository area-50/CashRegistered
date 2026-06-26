using System;

namespace Shared.Inventory.Response;

public class GetSearchInventoryTransactionResponse
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
