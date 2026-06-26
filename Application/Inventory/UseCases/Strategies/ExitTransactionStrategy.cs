using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class ExitTransactionStrategy(
    IStockBalanceRepository stockBalanceRepository,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceRepository, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.RequisitionExit || type == TransactionType.InventoryAdjustmentExit;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {
            var item = new InventoryTransactionItem(
                0, itemReq.ProductId, itemReq.UomId, itemReq.TransactionQuantity, 
                itemReq.BaseQuantity, itemReq.SourceWarehouseId, itemReq.DestinationWarehouseId);
            
            transaction.AddItem(item);

            if (itemReq.SourceWarehouseId == null)
            {
                _notificationContext.AddNotification("Warehouse", "Almoxarifado de origem é obrigatório para saída.");
                continue;
            }

            var balance = await GetStockBalanceAsync(itemReq.ProductId, itemReq.SourceWarehouseId.Value, isEntry: false);
            balance.RemoveStock(itemReq.BaseQuantity);
            
            if (balance.IsInvalid) 
            {
                _notificationContext.AddNotifications(balance.Notifications);
            }
            else 
            {
                _stockBalanceRepository.Update(balance);
            }
        }
    }
}
