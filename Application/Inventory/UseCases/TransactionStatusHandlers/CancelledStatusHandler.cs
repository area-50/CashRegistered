using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Shared.Inventory.Request;

namespace Application.Inventory.UseCases.TransactionStatusHandlers;

public class CancelledStatusHandler(
    IStockBalanceUseCase stockBalanceUseCase
) : BaseTransactionStatusHandler
{
    protected override bool CanHandle(InventoryTransaction transaction)
    {
        return transaction.Status == TransactionStatus.Cancelled;
    }

    protected override async Task HandleAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems)
    {
        foreach (var item in requestItems)
        {
            if (item.SourceWarehouseId.HasValue)
            {
                await stockBalanceUseCase.ReleaseStockReservationAsync(
                    item.ProductId, 
                    item.SourceWarehouseId.Value, 
                    item.BaseQuantity
                );
            }
        }
        
        await Task.CompletedTask;
    }
}
