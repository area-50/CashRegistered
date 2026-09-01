using Domain.Shared.DTOs;
using Domain.Shared.Response;

using Domain.Shared.DTOs;

namespace Application.Inventory.Interfaces;

public interface IInventoryTransactionUseCase
{
    Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request);
    
    Task<PagedResponse<GetSearchInventoryTransactionResponse>> SearchAsync(SearchInventoryTransactionRequest request);
    
    Task<GetInventoryTransactionByIdResponse?> GetByIdAsync(int id);
    
    Task<UpdateResponse> UpdateTransactionStatusAsync(int transactionId, string newStatusStr, int? sourceWarehouseId = null);
}
