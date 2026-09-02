using Domain.Financial.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;
using Domain.Shared.DTOs;

namespace Domain.Financial.Interfaces;

public interface ICostCenterRepository : IRepository<CostCenter>
{
    Task<PagedResponse<CostCenter>> SearchAsync(SearchCostCenterRequest request);
    Task<bool> ExistsByNameAsync(string name, int? ignoreId = null);
}
