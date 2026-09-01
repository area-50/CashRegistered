using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResponse<Product>> SearchAsync(SearchProductRequest request);
}