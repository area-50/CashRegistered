using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IInventoryRequisitionUseCase
{
    Task<CreateResponse> CreateRequisitionAsync(CreateInventoryRequisitionRequest request);
    
    Task<UpdateResponse> FulfillRequisitionAsync(int requisitionId, int fulfilledByUserId, FulfillInventoryRequisitionRequest request);
    
    Task<UpdateResponse> CancelRequisitionAsync(int requisitionId);
    
    Task<PagedResponse<SearchInventoryRequisitionResponse>> SearchAsync(SearchInventoryRequisitionRequest request);
    
    Task<GetInventoryRequisitionByIdResponse?> GetByIdAsync(int id);
    
    Task<int> GetPendingCountAsync();
}
