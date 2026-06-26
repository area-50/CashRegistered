using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class EntryTransactionStrategy(
    IStockBalanceRepository stockBalanceRepository,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceRepository, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.PurchaseEntry || type == TransactionType.InventoryAdjustmentEntry;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {
            var item = new InventoryTransactionItem(
                0, itemReq.ProductId, itemReq.UomId, itemReq.TransactionQuantity, 
                itemReq.BaseQuantity, itemReq.SourceWarehouseId, itemReq.DestinationWarehouseId);
            
            transaction.AddItem(item);

            if (itemReq.DestinationWarehouseId == null)
            {
                _notificationContext.AddNotification("Warehouse", "Almoxarifado de destino é obrigatório para entrada.");
                continue;
            }

            var balance = await GetStockBalanceAsync(itemReq.ProductId, itemReq.DestinationWarehouseId.Value, isEntry: true);
            balance.AddStock(itemReq.BaseQuantity);
            
            _stockBalanceRepository.Update(balance);
        }
    }
}
