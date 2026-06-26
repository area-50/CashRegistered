using Domain.Inventory.Entities;
using Shared.Abstractions;

using Shared.Response;
using Shared.Inventory.Request;

namespace Domain.Inventory.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
{
    Task<PagedResponse<InventoryTransaction>> SearchAsync(SearchInventoryTransactionRequest request);
}
