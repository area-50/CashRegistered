using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class InventoryTransactionUseCase(
    IInventoryTransactionRepository transactionRepository,
    IStockBalanceRepository stockBalanceRepository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IInventoryTransactionUseCase
{
    public async Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            notificationContext.AddNotification("Transaction", "A transação deve conter ao menos um item.");
            return new CreateResponse { Id = 0 };
        }

        if (!Enum.TryParse(request.TransactionType, out TransactionType type))
        {
            notificationContext.AddNotification("Transaction", "Tipo de transação inválido.");
            return new CreateResponse { Id = 0 };
        }

        var transaction = new InventoryTransaction(request.UserId, type, request.ReferenceDocument);

        foreach (var itemReq in request.Items)
        {
            var item = new InventoryTransactionItem(
                0, itemReq.ProductId, itemReq.UomId, itemReq.TransactionQuantity, 
                itemReq.BaseQuantity, itemReq.SourceWarehouseId, itemReq.DestinationWarehouseId);
            
            transaction.AddItem(item);

            if (type == TransactionType.PurchaseEntry)
            {
                if (itemReq.DestinationWarehouseId == null)
                {
                    notificationContext.AddNotification("Warehouse", "Almoxarifado de destino é obrigatório para entrada.");
                    continue;
                }
                var balance = await GetStockBalance(itemReq.ProductId, itemReq.DestinationWarehouseId.Value);
                balance.AddStock(itemReq.BaseQuantity);
                stockBalanceRepository.Update(balance);
            }
            else if (type == TransactionType.RequisitionExit)
            {
                if (itemReq.SourceWarehouseId == null)
                {
                    notificationContext.AddNotification("Warehouse", "Almoxarifado de origem é obrigatório para saída.");
                    continue;
                }
                var balance = await GetStockBalance(itemReq.ProductId, itemReq.SourceWarehouseId.Value);
                balance.RemoveStock(itemReq.BaseQuantity);
                
                if (balance.IsInvalid) notificationContext.AddNotifications(balance.Notifications);
                else stockBalanceRepository.Update(balance);
            }
        }

        if (notificationContext.Notifications.Any() || transaction.IsInvalid)
            return new CreateResponse { Id = 0 };

        await transactionRepository.CreateAsync(transaction);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = transaction.Id };
    }

    private async Task<StockBalance> GetStockBalance(int productId, int warehouseId)
    {
        var balances = await stockBalanceRepository.FindAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId);
        var balance = balances.FirstOrDefault();

        if (balance == null)
        {
            notificationContext.AddNotification("StockBalance", "Saldo não encontrado para o produto e almoxarifado.");
            return new StockBalance(productId, warehouseId); 
        }

        return balance;
    }
}
