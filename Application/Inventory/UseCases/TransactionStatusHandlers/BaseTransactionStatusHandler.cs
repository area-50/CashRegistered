using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Shared.DTOs;

namespace Application.Inventory.UseCases.TransactionStatusHandlers;

public abstract class BaseTransactionStatusHandler : ITransactionStatusHandler
{
    private ITransactionStatusHandler? _next;

    public void SetNext(ITransactionStatusHandler next)
    {
        _next = next;
    }

    public async Task ProcessAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems)
    {
        if (CanHandle(transaction))
        {
            await HandleAsync(transaction, requestItems);
        }
        else if (_next != null)
        {
            await _next.ProcessAsync(transaction, requestItems);
        }
    }

    protected abstract bool CanHandle(InventoryTransaction transaction);
    
    protected abstract Task HandleAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems);
}
