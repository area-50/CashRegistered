using Shared.Abstractions;
using Domain.Inventory.Enums;
using Domain.Identity.Entities;
using Flunt.Br;

namespace Domain.Inventory.Entities;

public class InventoryTransaction : BaseEntity
{
    protected InventoryTransaction() { } // EF Core

    public InventoryTransaction(
        int userId, 
        TransactionType type, 
        string? referenceDocument = null)
    {
        UserId = userId;
        Type = type;
        Status = TransactionStatus.Completed; // Padrão
        DateTime = DateTime.UtcNow;
        ReferenceDocument = referenceDocument;
        Items = new List<InventoryTransactionItem>();
        
        EntityValidate();
    }

    public DateTime DateTime { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? ReferenceDocument { get; private set; }
    
    public ICollection<InventoryTransactionItem> Items { get; private set; }

    public void AddItem(InventoryTransactionItem item)
    {
        Items.Add(item);
    }

    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsGreaterThan(UserId, 0, "Transação", "O Usuário responsável é obrigatório.");
            
        AddNotifications(contract.Notifications);
    }
}
