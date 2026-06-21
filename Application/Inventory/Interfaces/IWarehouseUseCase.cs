using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IWarehouseUseCase
{
    Task<IEnumerable<Warehouse>> ListAll();
    Task<CreateResponse> CreateWarehouse(CreateWarehouseRequest request);
    
    Task<PagedResponse<GetWarehouseByIdResponse>> SearchWarehouses(SearchWarehouseRequest request);
    
    Task<Warehouse?> GetById(int id);
    
    Task<GetWarehouseByIdResponse> GetWarehouseById(int id);

    Task<UpdateResponse> UpdateWarehouse(int id, UpdateWarehouseRequest request);
    
    Task DeactivateWarehouse(int id);
}
