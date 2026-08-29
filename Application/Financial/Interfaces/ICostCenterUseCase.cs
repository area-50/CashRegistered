using Shared.Abstractions;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Shared.Response;

namespace Application.Financial.Interfaces;

public interface ICostCenterUseCase
{
    Task<CreateResponse> CreateCostCenter(CreateCostCenterRequest request);
    Task UpdateCostCenter(int id, UpdateCostCenterRequest request);
    Task DeactivateCostCenter(int id);
    Task<GetCostCenterByIdResponse?> GetCostCenterById(int id);
    Task<PagedResponse<GetSearchCostCenterResponse>> SearchCostCenters(SearchCostCenterRequest request);
}
