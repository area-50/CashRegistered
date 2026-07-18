using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ICostCenterUseCase
{
    Task<CreateResponse> CreateCostCenter(CreateCostCenterRequest request);
    Task UpdateCostCenter(int id, UpdateCostCenterRequest request);
    Task DeactivateCostCenter(int id);
    Task<GetCostCenterByIdResponse?> GetCostCenterById(int id);
    Task<PagedResponse<GetSearchCostCenterResponse>> SearchCostCenters(SearchCostCenterRequest request);
}
