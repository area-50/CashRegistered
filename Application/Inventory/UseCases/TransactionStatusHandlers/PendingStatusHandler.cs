using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Shared.DTOs;

namespace Application.Inventory.UseCases.TransactionStatusHandlers;

public class PendingStatusHandler(
    IStockBalanceUseCase stockBalanceUseCase
) : BaseTransactionStatusHandler
{
    protected override bool CanHandle(InventoryTransaction transaction)
    {
        return transaction.Status == TransactionStatus.Pending;
    }

    protected override async Task HandleAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems)
    {
        foreach (var item in requestItems)
        {
            if (item.SourceWarehouseId.HasValue)
            {
                await stockBalanceUseCase.ReserveStockAsync(
                    item.ProductId, 
                    item.SourceWarehouseId.Value, 
                    item.BaseQuantity
                );
            }
        }
        
        await Task.CompletedTask;
    }
}
