using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<PagedResponse<Supplier>> SearchAsync(SearchSupplierRequest request);
}
