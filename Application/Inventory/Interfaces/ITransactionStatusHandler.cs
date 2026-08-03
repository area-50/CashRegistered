using Domain.Inventory.Entities;
using Shared.Inventory.Request;

namespace Application.Inventory.Interfaces;

public interface ITransactionStatusHandler
{
    void SetNext(ITransactionStatusHandler next);
    Task ProcessAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems);
}
