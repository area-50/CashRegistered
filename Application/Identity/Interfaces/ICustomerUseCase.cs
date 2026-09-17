using Domain.Shared.Abstractions;
using Shared.Identity.Request;
using Shared.Identity.Response;

namespace Application.Identity.Interfaces;

public interface ICustomerUseCase
{
    Task<CreateResponse> CreateCustomer(CreateCustomerRequest request);
    Task<GetCustomerByIdResponse?> GetCustomerById(int id);
    Task<PagedResponse<GetSearchCustomerResponse>> SearchCustomers(Shared.Identity.Request.SearchCustomerRequest request);
    Task UpdateCustomer(int id, UpdateCustomerRequest request);
    Task DeactivateCustomer(int id);
}
