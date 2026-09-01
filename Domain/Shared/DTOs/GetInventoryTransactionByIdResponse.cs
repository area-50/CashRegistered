using System;
using System.Collections.Generic;

namespace Domain.Shared.DTOs;

public class GetInventoryTransactionByIdResponse
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TransactionStatus { get; set; } = string.Empty;
    public List<InventoryTransactionItemResponse> Items { get; set; } = new();
}
