using Shared.Inventory.Request;
using Shared.Response;

using Shared.Inventory.Response;

namespace Application.Inventory.Interfaces;

public interface IInventoryTransactionUseCase
{
    Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request);
    
    Task<PagedResponse<GetSearchInventoryTransactionResponse>> SearchAsync(SearchInventoryTransactionRequest request);
    
    Task<Shared.Inventory.Response.GetInventoryTransactionByIdResponse?> GetByIdAsync(int id);
    
    Task<UpdateResponse> UpdateTransactionStatusAsync(int transactionId, string newStatusStr, int? sourceWarehouseId = null);
}
