using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class EntryTransactionStrategy(
    IStockBalanceUseCase stockBalanceUseCase,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceUseCase, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.PurchaseEntry || type == TransactionType.InventoryAdjustmentEntry;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {

            if (itemReq.DestinationWarehouseId == null)
            {
                NotificationContext.AddNotification("Warehouse", "Almoxarifado de destino é obrigatório para entrada.");
                continue;
            }

            var balance = await StockBalanceUseCase.GetStockBalanceAsync(itemReq.ProductId, itemReq.DestinationWarehouseId.Value, isEntry: true);
            balance.AddStock(itemReq.BaseQuantity);
            
            StockBalanceUseCase.Update(balance);
        }
    }
}
