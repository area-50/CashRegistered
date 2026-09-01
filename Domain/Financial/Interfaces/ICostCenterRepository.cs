using Domain.Financial.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Financial.Interfaces;

public interface ICostCenterRepository : IRepository<CostCenter>
{
    Task<PagedResponse<CostCenter>> SearchAsync(SearchCostCenterRequest request);
}
