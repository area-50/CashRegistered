using Domain.Inventory.Entities;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResponse<Product>> SearchAsync(SearchProductRequest request);
}