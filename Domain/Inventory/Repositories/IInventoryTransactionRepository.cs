using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;

using Domain.Shared.Response;


namespace Domain.Inventory.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
{
    Task<PagedResponse<InventoryTransaction>> SearchAsync(SearchInventoryTransactionRequest request);
    Task<GetInventoryTransactionByIdResponse?> GetDetailsAsync(int id);
}
