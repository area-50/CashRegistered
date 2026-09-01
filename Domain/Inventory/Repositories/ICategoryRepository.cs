using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<PagedResponse<Category>> SearchAsync(SearchCategoryRequest request);
}