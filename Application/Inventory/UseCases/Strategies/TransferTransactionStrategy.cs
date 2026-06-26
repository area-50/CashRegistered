using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public class TransferTransactionStrategy(
    IStockBalanceRepository stockBalanceRepository,
    NotificationContext notificationContext
) : BaseInventoryTransactionStrategy(stockBalanceRepository, notificationContext)
{
    public override bool AppliesTo(TransactionType type)
    {
        return type == TransactionType.Transfer;
    }

    public override async Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items)
    {
        foreach (var itemReq in items)
        {
            var item = new InventoryTransactionItem(
                0, itemReq.ProductId, itemReq.UomId, itemReq.TransactionQuantity, 
                itemReq.BaseQuantity, itemReq.SourceWarehouseId, itemReq.DestinationWarehouseId);
            
            transaction.AddItem(item);

            if (itemReq.SourceWarehouseId == null || itemReq.DestinationWarehouseId == null)
            {
                _notificationContext.AddNotification("Warehouse", "Origem e destino são obrigatórios para transferências.");
                continue;
            }

            // 1. Remover da Origem
            var sourceBalance = await GetStockBalanceAsync(itemReq.ProductId, itemReq.SourceWarehouseId.Value, isEntry: false);
            sourceBalance.RemoveStock(itemReq.BaseQuantity);
            
            if (sourceBalance.IsInvalid) 
            {
                _notificationContext.AddNotifications(sourceBalance.Notifications);
                continue; // Interrompe para não adicionar no destino indevidamente
            }
            
            _stockBalanceRepository.Update(sourceBalance);

            // 2. Adicionar ao Destino
            var destinationBalance = await GetStockBalanceAsync(itemReq.ProductId, itemReq.DestinationWarehouseId.Value, isEntry: true);
            destinationBalance.AddStock(itemReq.BaseQuantity);
            
            _stockBalanceRepository.Update(destinationBalance);
        }
    }
}
