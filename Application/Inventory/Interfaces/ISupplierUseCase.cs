using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ISupplierUseCase
{
    Task<CreateResponse> CreateSupplier(CreateSupplierRequest request);
    Task<GetSupplierByIdResponse?> GetSupplierById(int id);
    Task<PagedResponse<GetSearchSupplierResponse>> SearchSuppliers(SearchSupplierRequest request);
    Task UpdateSupplier(int id, UpdateSupplierRequest request);
    Task DeactivateSupplier(int id);
}
