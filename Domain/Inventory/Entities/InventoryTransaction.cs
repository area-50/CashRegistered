using Domain.Shared.Abstractions;
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
        string? referenceDocument = null,
        string? name = null,
        string? description = null,
        TransactionStatus status = TransactionStatus.Completed)
    {
        UserId = userId;
        Type = type;
        Status = status; // Padrão agora pode ser injetado
        DateTime = DateTime.UtcNow;
        ReferenceDocument = referenceDocument;
        Name = name;
        Description = description;
        Items = new List<InventoryTransactionItem>();
        
        EntityValidate();
    }

    public DateTime DateTime { get; private set; }
    
    public int UserId { get; private set; }
    
    public User User { get; private set; }
    
    public TransactionType Type { get; private set; }
    
    public TransactionStatus Status { get; private set; }
    
    public string? ReferenceDocument { get; private set; }
    
    public string? Name { get; private set; }
    
    public string? Description { get; private set; }
    
    
    public ICollection<InventoryTransactionItem> Items { get; private set; }

    public void AddItem(InventoryTransactionItem item)
    {
        if (item.IsInvalid) AddNotifications(item.Notifications);
        Items.Add(item);
    }

    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsGreaterThan(UserId, 0, "Transação", "O Usuário responsável é obrigatório.");
            
        AddNotifications(contract.Notifications);
    }

    public void Fulfill()
    {
        if (Status != TransactionStatus.Pending)
        {
            AddNotification("Transaction", "Apenas transações pendentes podem ser atendidas.");
            return;
        }
        Status = TransactionStatus.Completed;
    }

    public void Cancel()
    {
        if (Status != TransactionStatus.Pending)
        {
            AddNotification("Transaction", "Apenas transações pendentes podem ser canceladas.");
            return;
        }
        Status = TransactionStatus.Cancelled;
    }
}
