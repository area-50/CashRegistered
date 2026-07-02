using Shared.Inventory.Request;
using Shared.Response;

using Shared.Inventory.Response;

namespace Application.Inventory.Interfaces;

public interface IInventoryTransactionUseCase
{
    Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request);
    
    Task<PagedResponse<GetSearchInventoryTransactionResponse>> SearchAsync(SearchInventoryTransactionRequest request);
    
    Task<GetInventoryTransactionByIdResponse?> GetByIdAsync(int id);
}
