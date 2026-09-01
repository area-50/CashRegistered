using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<PagedResponse<Warehouse>> SearchAsync(SearchWarehouseRequest request);
}
