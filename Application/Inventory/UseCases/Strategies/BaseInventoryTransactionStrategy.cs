using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;

namespace Application.Inventory.UseCases.Strategies;

public abstract class BaseInventoryTransactionStrategy(
    IStockBalanceUseCase stockBalanceUseCase,
    NotificationContext notificationContext
) : IInventoryTransactionStrategy
{
    protected readonly IStockBalanceUseCase StockBalanceUseCase = stockBalanceUseCase;
    protected readonly NotificationContext NotificationContext = notificationContext;

    public abstract bool AppliesTo(TransactionType type);

    public abstract Task ProcessTransactionAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> items);
}
