using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Response;

namespace Domain.Inventory.Interfaces;

public interface ICostCenterRepository : IRepository<CostCenter>
{
    Task<PagedResponse<CostCenter>> SearchAsync(SearchCostCenterRequest request);
}
