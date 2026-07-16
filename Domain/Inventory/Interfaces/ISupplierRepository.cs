using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Response;

namespace Domain.Inventory.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<PagedResponse<Supplier>> SearchAsync(SearchSupplierRequest request);
}
