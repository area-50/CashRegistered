using Domain.Inventory.Entities;
using Shared.Abstractions;

using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IInventoryRequisitionRepository : IRepository<InventoryRequisition>
{
    Task<PagedResponse<SearchInventoryRequisitionResponse>> SearchAsync(SearchInventoryRequisitionRequest request);
    Task<GetInventoryRequisitionByIdResponse?> GetByIdResponseAsync(int id);
}
