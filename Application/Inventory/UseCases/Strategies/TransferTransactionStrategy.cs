using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class TransferTransactionStrategy(
    IStockBalanceUseCase stockBalanceUseCase,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceUseCase, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.Transfer;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {

            if (itemReq.SourceWarehouseId == null || itemReq.DestinationWarehouseId == null)
            {
                NotificationContext.AddNotification("Warehouse", "Origem e destino são obrigatórios para transferências.");
                continue;
            }

            // 1. Remover da Origem
            var sourceBalance = await StockBalanceUseCase.GetStockBalanceAsync(itemReq.ProductId, itemReq.SourceWarehouseId.Value, isEntry: false);
            sourceBalance.RemoveStock(itemReq.BaseQuantity);
            
            if (sourceBalance.IsInvalid) 
            {
                NotificationContext.AddNotifications(sourceBalance.Notifications);
                continue; // Interrompe para não adicionar no destino indevidamente
            }
            
            StockBalanceUseCase.Update(sourceBalance);

            // 2. Adicionar ao Destino
            var destinationBalance = await StockBalanceUseCase.GetStockBalanceAsync(itemReq.ProductId, itemReq.DestinationWarehouseId.Value, isEntry: true);
            destinationBalance.AddStock(itemReq.BaseQuantity);
            
            StockBalanceUseCase.Update(destinationBalance);
        }
    }
}
