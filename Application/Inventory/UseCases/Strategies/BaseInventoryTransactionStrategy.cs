using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public abstract class BaseInventoryTransactionStrategy(
    IStockBalanceRepository stockBalanceRepository,
    NotificationContext notificationContext
) : IInventoryTransactionStrategy
{
    protected readonly IStockBalanceRepository _stockBalanceRepository = stockBalanceRepository;
    protected readonly NotificationContext _notificationContext = notificationContext;

    public abstract bool AppliesTo(TransactionType type);

    public abstract Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items);

    protected async Task<StockBalance> GetStockBalanceAsync(int productId, int warehouseId, bool isEntry = false)
    {
        var balances = await _stockBalanceRepository.FindAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId);
        var balance = balances.FirstOrDefault();

        if (balance == null)
        {
            if (!isEntry)
            {
                _notificationContext.AddNotification("StockBalance", "Saldo não encontrado na origem para este produto.");
            }
            return new StockBalance(productId, warehouseId); 
        }

        return balance;
    }
}
