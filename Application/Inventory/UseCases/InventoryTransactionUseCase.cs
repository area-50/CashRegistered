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
    IEnumerable<IInventoryTransactionStrategy> strategies,
    IInventoryTransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IInventoryTransactionUseCase
{
    public async Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request)
    {
        if (!request.Items.Any())
        {
            notificationContext.AddNotification("Transaction", "A transação deve conter ao menos um item.");
            return new CreateResponse { Id = 0 };
        }

        if (!Enum.TryParse(request.TransactionType, out TransactionType type))
        {
            notificationContext.AddNotification("Transaction", "Tipo de transação inválido.");
            return new CreateResponse { Id = 0 };
        }

        var transaction = new InventoryTransaction(
            request.UserId, 
            type, 
            request.ReferenceDocument, 
            request.Name, 
            request.Description);

        var strategy = strategies.FirstOrDefault(s => s.AppliesTo(type));
        
        if (strategy == null)
        {
            notificationContext.AddNotification("Transaction", "Nenhuma estratégia encontrada para este tipo de transação.");
            return new CreateResponse { Id = 0 };
        }

        await strategy.ProcessTransactionAsync(transaction, request.Items);

        if (notificationContext.Notifications.Any() || transaction.IsInvalid)
            return new CreateResponse { Id = 0 };

        await transactionRepository.CreateAsync(transaction);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = transaction.Id };
    }

    public async Task<PagedResponse<Shared.Inventory.Response.GetSearchInventoryTransactionResponse>> SearchAsync(SearchInventoryTransactionRequest request)
    {
        var pagedTransactions = await transactionRepository.SearchAsync(request);

        var responseItems = pagedTransactions.Items.Select(x => new Shared.Inventory.Response.GetSearchInventoryTransactionResponse
        {
            Id = x.Id,
            TransactionType = x.Type.ToString(),
            ReferenceDocument = x.ReferenceDocument,
            Name = x.Name,
            Description = x.Description,
            CreatedAt = x.DateTime,
            IsActive = true // Mandatório
        }).ToList();

        return new PagedResponse<Shared.Inventory.Response.GetSearchInventoryTransactionResponse>
        {
            Items = responseItems,
            TotalCount = pagedTransactions.TotalCount,
            Page = pagedTransactions.Page,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<Shared.Inventory.Response.GetInventoryTransactionByIdResponse?> GetByIdAsync(int id)
    {
        var transaction = await transactionRepository.GetByIdAsync(id);

        if (transaction == null)
        {
            return null;
        }

        return new Shared.Inventory.Response.GetInventoryTransactionByIdResponse
        {
            Id = transaction.Id,
            TransactionType = transaction.Type.ToString(),
            ReferenceDocument = transaction.ReferenceDocument,
            Name = transaction.Name,
            Description = transaction.Description,
            CreatedAt = transaction.DateTime,
            IsActive = true, // Mandatório
            Items = transaction.Items.Select(i => new Shared.Inventory.Response.InventoryTransactionItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.TransactionQuantity,
                SourceWarehouseId = i.SourceWarehouseId,
                SourceWarehouseName = i.SourceWarehouse?.Name,
                DestinationWarehouseId = i.DestinationWarehouseId,
                DestinationWarehouseName = i.DestinationWarehouse?.Name
            }).ToList()
        };
    }
}
