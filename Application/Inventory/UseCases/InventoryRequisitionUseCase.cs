using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class InventoryRequisitionUseCase(
    IInventoryRequisitionRepository repository,
    IInventoryTransactionUseCase transactionUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IInventoryRequisitionUseCase
{
    public async Task<CreateResponse> CreateRequisitionAsync(CreateInventoryRequisitionRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            notificationContext.AddNotification("Items", "A requisição deve conter ao menos um item.");
            return new CreateResponse { Id = 0 };
        }

        var requisition = new InventoryRequisition(request.OriginModule, request.RequestedByUserId, request.Notes);

        foreach (var item in request.Items)
        {
            requisition.AddItem(item.ProductId, item.Quantity);
        }

        if (requisition.IsInvalid || notificationContext.Notifications.Any())
            return new CreateResponse { Id = 0 };

        await repository.CreateAsync(requisition);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = requisition.Id };
    }

    public async Task<UpdateResponse> FulfillRequisitionAsync(int requisitionId, int fulfilledByUserId, FulfillInventoryRequisitionRequest request)
    {
        var requisition = await repository.GetByIdAsync(requisitionId);
        if (requisition == null)
        {
            notificationContext.AddNotification("Requisition", "Requisição não encontrada.");
            return new UpdateResponse { Id = 0 };
        }

        // Action on Domain
        requisition.Fulfill();

        if (requisition.IsInvalid || notificationContext.Notifications.Any())
            return new UpdateResponse { Id = 0 };

        // Cross-domain call (using use case, NOT cross-repository)
        var transactionRequest = new CreateInventoryTransactionRequest
        {
            UserId = fulfilledByUserId,
            TransactionType = "RequisitionExit",
            ReferenceDocument = $"REQ-{requisition.Id}",
            Name = $"Atendimento Requisição {requisition.Id}",
            Description = $"Origem: {requisition.OriginModule}",
            Items = requisition.Items.Select(x => new CreateInventoryTransactionItemRequest
            {
                ProductId = x.ProductId,
                UomId = x.Product.BaseUomId,
                TransactionQuantity = x.Quantity,
                BaseQuantity = x.Quantity,
                SourceWarehouseId = request.SourceWarehouseId
            }).ToList()
        };

        var transactionResponse = await transactionUseCase.CreateTransaction(transactionRequest);

        if (transactionResponse.Id == 0)
        {
            // The transaction use case failed, it already added notifications.
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(requisition);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = requisition.Id };
    }

    public async Task<UpdateResponse> CancelRequisitionAsync(int requisitionId)
    {
        var requisition = await repository.GetByIdAsync(requisitionId);
        if (requisition == null)
        {
            notificationContext.AddNotification("Requisition", "Requisição não encontrada.");
            return new UpdateResponse { Id = 0 };
        }

        requisition.Cancel();

        if (requisition.IsInvalid || notificationContext.Notifications.Any())
            return new UpdateResponse { Id = 0 };

        repository.Update(requisition);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = requisition.Id };
    }

    public async Task<PagedResponse<SearchInventoryRequisitionResponse>> SearchAsync(SearchInventoryRequisitionRequest request)
    {
        return await repository.SearchAsync(request);
    }

    public async Task<GetInventoryRequisitionByIdResponse?> GetByIdAsync(int id)
    {
        return await repository.GetByIdResponseAsync(id);
    }
}
