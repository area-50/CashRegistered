using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Shared.Inventory.Request;

namespace Application.Inventory.Interfaces;

public interface IInventoryTransactionStrategy
{
    bool AppliesTo(TransactionType type);
    Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items);
}
