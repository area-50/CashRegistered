using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class InventoryTransactionUseCase(
    ITransactionStatusHandler statusChain,
    IInventoryTransactionRepository transactionRepository,
    IProductUseCase productUseCase,
    IStockBalanceUseCase stockBalanceUseCase,
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

        TransactionStatus status = TransactionStatus.Completed;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse(request.Status, out TransactionStatus parsedStatus))
        {
            status = parsedStatus;
        }

        foreach (var item in request.Items)
        {
            var product = await productUseCase.GetById(item.ProductId);
            if (product == null)
            {
                notificationContext.AddNotification("Product", "Produto não encontrado.");
                return new CreateResponse { Id = 0 };
            }

            if (item.UomId != product.BaseUomId)
            {
                var productConversions = await productUseCase.GetProductConversions(product.Id);
                var matchingConversions = productConversions
                    .Where(c => c.UomId == item.UomId)
                    .ToList();

                if (!matchingConversions.Any())
                {
                    notificationContext.AddNotification("UomConversion", $"Nenhuma regra de conversão encontrada na árvore para {product.Name}");
                    return new CreateResponse { Id = 0 };
                }

                bool isValidMath = matchingConversions.Any(c => 
                    item.BaseQuantity == item.TransactionQuantity * c.Multiplier);
                
                if (!isValidMath)
                {
                    notificationContext.AddNotification(
                        "BaseQuantity",
                        $"Valores corrompidos ou inconsistentes. A conversão submetida não se encaixa nas regras ativas do produto."
                    );
                    return new CreateResponse { Id = 0 };
                }
            }
            else 
            {
                if (item.BaseQuantity != item.TransactionQuantity)
                {
                     notificationContext.AddNotification(
                         "BaseQuantity",
                         "Quantidade base deve ser igual à quantidade da transação quando as unidades são idênticas."
                    );
                     return new CreateResponse { Id = 0 };
                }
            }
        }

        var transaction = new InventoryTransaction(
            request.UserId, 
            type, 
            request.ReferenceDocument, 
            request.Name, 
            request.Description,
            status
        );

        foreach (var itemReq in request.Items)
        {
            var item = new InventoryTransactionItem(
                0, itemReq.ProductId, itemReq.UomId, itemReq.TransactionQuantity, 
                itemReq.BaseQuantity, itemReq.SourceWarehouseId, itemReq.DestinationWarehouseId
             );
            
            transaction.AddItem(item);
        }

        await statusChain.ProcessAsync(transaction, request.Items);

        if (notificationContext.Notifications.Any() || transaction.IsInvalid)
            return new CreateResponse { Id = 0 };

        await transactionRepository.CreateAsync(transaction);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = transaction.Id };
    }

    public async Task<PagedResponse<GetSearchInventoryTransactionResponse>> SearchAsync(SearchInventoryTransactionRequest request)
    {
        var pagedTransactions = await transactionRepository.SearchAsync(request);

        var responseItems = pagedTransactions.Items.Select(x => new GetSearchInventoryTransactionResponse
        {
            Id = x.Id,
            TransactionType = x.Type.ToString(),
            ReferenceDocument = x.ReferenceDocument,
            Name = x.Name,
            Description = x.Description,
            TransactionDate = x.DateTime,
            TransactionStatus = x.Status.ToString()
        }).ToList();

        return new PagedResponse<GetSearchInventoryTransactionResponse>
        {
            Items = responseItems,
            TotalCount = pagedTransactions.TotalCount,
            Page = pagedTransactions.Page,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<GetInventoryTransactionByIdResponse?> GetByIdAsync(int id)
    {
        return await transactionRepository.GetDetailsAsync(id);
    }

    public async Task<UpdateResponse> UpdateTransactionStatusAsync(int transactionId, string newStatusStr, int? sourceWarehouseId = null)
    {
        var transaction = await transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            notificationContext.AddNotification("Transaction", "Transação não encontrada.");
            return new UpdateResponse { Id = 0 };
        }

        if (!Enum.TryParse(newStatusStr, out TransactionStatus newStatus))
        {
            notificationContext.AddNotification("Status", "Status inválido.");
            return new UpdateResponse { Id = 0 };
        }

        var oldStatus = transaction.Status;

        // Captura o estado original dos itens para saber se houve reserva prévia (apenas se tinham SourceWarehouseId)
        var originallyReservedItems = transaction.Items
            .Where(i => i.SourceWarehouseId.HasValue)
            .Select(i => new { i.ProductId, WarehouseId = i.SourceWarehouseId.Value, i.BaseQuantity })
            .ToList();

        if (newStatus == TransactionStatus.Completed)
        {
            if (sourceWarehouseId.HasValue && transaction.Type == TransactionType.RequisitionExit)
            {
                // Injetar o Almoxarifado fornecido no atendimento da requisição
                foreach (var item in transaction.Items)
                {
                    item.UpdateSourceWarehouse(sourceWarehouseId.Value);
                }
            }
            transaction.Fulfill();
        }
        else if (newStatus == TransactionStatus.Cancelled)
        {
            transaction.Cancel();
        }
        else
        {
            notificationContext.AddNotification("Status", "Transição de status não suportada.");
            return new UpdateResponse { Id = 0 };
        }

        if (transaction.IsInvalid || notificationContext.Notifications.Any())
            return new UpdateResponse { Id = 0 };

        if (oldStatus == TransactionStatus.Pending && newStatus == TransactionStatus.Completed)
        {
            var requestItems = transaction.Items.Select(x => new CreateInventoryTransactionItemRequest
            {
                ProductId = x.ProductId,
                UomId = x.UomId,
                TransactionQuantity = x.TransactionQuantity,
                BaseQuantity = x.BaseQuantity,
                SourceWarehouseId = x.SourceWarehouseId,
                DestinationWarehouseId = x.DestinationWarehouseId
            }).ToList();

            // Consumir apenas o saldo que foi EFETIVAMENTE reservado no momento do Pending
            foreach (var reservedItem in originallyReservedItems)
            {
                await stockBalanceUseCase.ConsumeStockReservationAsync(
                    reservedItem.ProductId, reservedItem.WarehouseId, reservedItem.BaseQuantity);
            }
            
            await statusChain.ProcessAsync(transaction, requestItems);
        }
        else if (oldStatus == TransactionStatus.Pending && newStatus == TransactionStatus.Cancelled)
        {
            var requestItems = transaction.Items.Select(x => new CreateInventoryTransactionItemRequest
            {
                ProductId = x.ProductId,
                UomId = x.UomId,
                TransactionQuantity = x.TransactionQuantity,
                BaseQuantity = x.BaseQuantity,
                SourceWarehouseId = x.SourceWarehouseId,
                DestinationWarehouseId = x.DestinationWarehouseId
            }).ToList();
            
            await statusChain.ProcessAsync(transaction, requestItems);
        }

        // Previne o commit caso o ProcessAsync (Strategies) tenha gerado notificações de erro (ex: saldo insuficiente)
        if (notificationContext.Notifications.Any())
            return new UpdateResponse { Id = 0 };

        transactionRepository.Update(transaction);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = transaction.Id };
    }
}
