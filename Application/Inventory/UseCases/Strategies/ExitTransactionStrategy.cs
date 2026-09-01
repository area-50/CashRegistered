using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class ExitTransactionStrategy(
    IStockBalanceUseCase stockBalanceUseCase,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceUseCase, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.RequisitionExit || type == TransactionType.InventoryAdjustmentExit;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {

            if (itemReq.SourceWarehouseId == null)
            {
                NotificationContext.AddNotification("Warehouse", "Almoxarifado de origem é obrigatório para saída.");
                continue;
            }

            var balance = await StockBalanceUseCase.GetStockBalanceAsync(itemReq.ProductId, itemReq.SourceWarehouseId.Value, isEntry: false);
            balance.RemoveStock(itemReq.BaseQuantity);
            
            if (balance.IsInvalid) 
            {
                NotificationContext.AddNotifications(balance.Notifications);
            }
            else 
            {
                StockBalanceUseCase.Update(balance);
            }
        }
    }
}
