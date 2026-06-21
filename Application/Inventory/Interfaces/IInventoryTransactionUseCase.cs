using Shared.Inventory.Request;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IInventoryTransactionUseCase
{
    Task<CreateResponse> CreateTransaction(CreateInventoryTransactionRequest request);
}
