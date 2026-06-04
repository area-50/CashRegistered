using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<PagedResponse<Warehouse>> SearchAsync(SearchWarehouseRequest request);
}
