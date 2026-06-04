using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ICategoryUseCase
{
    Task<CreateResponse> CreateCategory(CreateCategoryRequest request);
    
    Task<PagedResponse<GetSearchCategoryResponse>> GetSearchCategories(SearchCategoryRequest request);

    Task DeactivateCategory(int categoryId);
    
    Task<Category?> GetCategoryById(int categoryId);
}