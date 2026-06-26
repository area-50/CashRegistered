using System;
using System.Collections.Generic;

namespace Shared.Inventory.Response;

public class GetInventoryTransactionByIdResponse
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<InventoryTransactionItemResponse> Items { get; set; } = new();
}
