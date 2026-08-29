using Domain.Financial.Entities;
using Shared.Abstractions;
using Shared.Financial.Request;
using Shared.Response;

namespace Domain.Financial.Interfaces;

public interface ICostCenterRepository : IRepository<CostCenter>
{
    Task<PagedResponse<CostCenter>> SearchAsync(SearchCostCenterRequest request);
}
