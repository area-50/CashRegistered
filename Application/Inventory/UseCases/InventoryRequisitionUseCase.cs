using Application.Inventory.Interfaces;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Domain.Shared.Interfaces;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class InventoryRequisitionUseCase(
    IInventoryTransactionUseCase transactionUseCase,
    IInventoryTransactionRepository transactionRepository,
    NotificationContext notificationContext,
    IEventDispatcher dispatcher,
    IProductUseCase productUseCase
) : IInventoryRequisitionUseCase
{
    public async Task<CreateResponse> CreateRequisitionAsync(CreateInventoryRequisitionRequest request)
    {
        if (!request.Items.Any())
        {
            notificationContext.AddNotification("Items", "A requisição deve conter ao menos um item.");
            return new CreateResponse { Id = 0 };
        }

        var transactionItems = new List<CreateInventoryTransactionItemRequest>();
        foreach (var item in request.Items)
        {
            var product = await productUseCase.GetById(item.ProductId);
            if (product != null)
            {
                transactionItems.Add(new CreateInventoryTransactionItemRequest
                {
                    ProductId = item.ProductId,
                    UomId = product.BaseUomId,
                    TransactionQuantity = item.Quantity,
                    BaseQuantity = item.Quantity,
                    SourceWarehouseId = null
                });
            }
        }

        var transactionRequest = new CreateInventoryTransactionRequest
        {
            UserId = request.RequestedByUserId,
            TransactionType = "RequisitionExit",
            ReferenceDocument = $"REQ-{request.OriginModule}", // We use reference document to store origin
            Name = $"REQ:{request.OriginModule}",
            Description = request.Notes,
            Status = "Pending",
            Items = transactionItems
        };

        var response = await transactionUseCase.CreateTransaction(transactionRequest);

        if (response.Id > 0)
        {
            await dispatcher.Publish(new Domain.Inventory.Events.RequisitionStatusChangedEvent(
                response.Id, 
                "Pending"
            ));
            return new CreateResponse {Id = response.Id};
        }
        return new CreateResponse {Id = 0};
    }

    public async Task<UpdateResponse> FulfillRequisitionAsync(
        int requisitionId, int fulfilledByUserId, FulfillInventoryRequisitionRequest request
    )
    {
        var response = await transactionUseCase.UpdateTransactionStatusAsync(requisitionId, "Completed", request.SourceWarehouseId);
        if (response.Id > 0)
        {
            await dispatcher.Publish(new Domain.Inventory.Events.RequisitionStatusChangedEvent(
                response.Id, 
                "Completed"
            ));
        }
        return response;
    }

    public async Task<UpdateResponse> CancelRequisitionAsync(int requisitionId)
    {
        var response = await transactionUseCase.UpdateTransactionStatusAsync(requisitionId, "Cancelled");
        if (response.Id > 0)
        {
            await dispatcher.Publish(new Domain.Inventory.Events.RequisitionStatusChangedEvent(
                response.Id, 
                "Cancelled"
            ));
        }
        return response;
    }

    public async Task<PagedResponse<SearchInventoryRequisitionResponse>> SearchAsync(SearchInventoryRequisitionRequest request)
    {
        var searchRequest = new SearchInventoryTransactionRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TransactionType = "RequisitionExit",
            TransactionStatus = request.Status,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var pagedTransactions = await transactionRepository.SearchAsync(searchRequest);

        var mappedItems = pagedTransactions.Items.Select(t => new SearchInventoryRequisitionResponse
        {
            Id = t.Id,
            OriginModule = t.Name.StartsWith("REQ:") ? t.Name.Substring(4) : t.Name,
            RequestedByUserId = t.UserId,
            RequestedByUserName = t.User?.UserName ?? "Desconhecido",
            Status = t.Status.ToString(),
            CreatedAt = t.DateTime,
            FulfilledAt = t.Status == TransactionStatus.Completed ? t.DateTime : null,
            IsActive = t.IsActive
        }).ToList();

        return new PagedResponse<SearchInventoryRequisitionResponse>
        {
            Items = mappedItems,
            TotalCount = pagedTransactions.TotalCount,
            Page = pagedTransactions.Page,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<GetInventoryRequisitionByIdResponse?> GetByIdAsync(int id)
    {
        var t = await transactionRepository.GetDetailsAsync(id);
        if (t == null) return null;

        var originModule = t.Name.StartsWith("REQ:") ? t.Name.Substring(4) : t.Name;

        // Tentar obter a entidade base para pegar o User
        var baseTransaction = await transactionRepository.GetByIdAsync(id);

        return new GetInventoryRequisitionByIdResponse
        {
            Id = t.Id,
            OriginModule = originModule,
            RequestedByUserId = baseTransaction?.UserId ?? 0,
            RequestedByUserName = baseTransaction?.User?.UserName ?? "Desconhecido",
            Status = t.TransactionStatus,
            CreatedAt = t.CreatedAt,
            FulfilledAt = t.TransactionStatus == "Completed" ? t.CreatedAt : null,
            Notes = t.Description,
            Items = t.Items.Select(i => new GetInventoryRequisitionItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task<int> GetPendingCountAsync()
    {
        var searchRequest = new SearchInventoryTransactionRequest
        {
            Page = 1,
            PageSize = 1,
            TransactionType = "RequisitionExit",
            TransactionStatus = "Pending"
        };
        var result = await transactionRepository.SearchAsync(searchRequest);
        return result.TotalCount;
    }
}
