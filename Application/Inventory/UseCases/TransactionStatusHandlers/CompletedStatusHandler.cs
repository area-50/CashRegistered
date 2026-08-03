using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Application.Inventory.UseCases.TransactionStatusHandlers;

public class CompletedStatusHandler(
    IEnumerable<IInventoryTransactionStrategy> strategies,
    NotificationContext notificationContext
) : BaseTransactionStatusHandler
{
    protected override bool CanHandle(InventoryTransaction transaction)
    {
        return transaction.Status == TransactionStatus.Completed;
    }

    protected override async Task HandleAsync(InventoryTransaction transaction, IEnumerable<CreateInventoryTransactionItemRequest> requestItems)
    {
        var strategy = strategies.FirstOrDefault(s => s.AppliesTo(transaction.Type));
        
        if (strategy == null)
        {
            notificationContext.AddNotification("Transaction", "Nenhuma estratégia de movimentação física encontrada para este tipo.");
            return;
        }

        await strategy.ProcessTransactionAsync(transaction, requestItems);
    }
}
