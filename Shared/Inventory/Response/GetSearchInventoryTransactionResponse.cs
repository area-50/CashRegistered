using System;

namespace Shared.Inventory.Response;

public class GetSearchInventoryTransactionResponse
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Reflete a data real em que a transação ocorreu/foi registrada (DateTime da entidade InventoryTransaction)
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    public bool IsActive { get; set; }
}
