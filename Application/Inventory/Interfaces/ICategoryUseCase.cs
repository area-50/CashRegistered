using Domain.Inventory.Entities;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ICategoryUseCase
{
    Task<CreateResponse> CreateCategory(CreateCategoryRequest request);
    
    Task<PagedResponse<GetSearchCategoryResponse>> GetSearchCategories(SearchCategoryRequest request);

    Task DeactivateCategory(int categoryId);
    
    Task<Category?> GetCategoryById(int categoryId);

    Task<GetCategoryByIdResponse> GetCategoryByIdResponse(int categoryId);

    Task<UpdateResponse> UpdateCategory(int id, UpdateCategoryRequest request);
}