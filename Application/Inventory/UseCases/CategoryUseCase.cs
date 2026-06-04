using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class CategoryUseCase(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : ICategoryUseCase
{
    public async Task<CreateResponse> CreateCategory(CreateCategoryRequest request)
    {
        var category = new Category(
            request.Name,
            request.ParentCategoryId
        );

        if (category.IsInvalid)
        {
            notificationContext.AddNotifications(category.Notifications);
            return new CreateResponse
            {
                Id = 0
            };
        }
        
        await repository.CreateAsync(category);
        await unitOfWork.CommitAsync();

        return new CreateResponse
        {
            Id = category.Id
        };
    }

    public async Task<PagedResponse<GetSearchCategoryResponse>> GetSearchCategories(SearchCategoryRequest request)
    {
        var pagedCategories = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchCategoryResponse>
        {
            Items = pagedCategories.Items.Select(c => new GetSearchCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryName = c.ParentCategory?.Name,
                    ParentCategoryId = c.ParentCategory?.Id,
                    IsActive =  c.IsActive
                }
            ),
            Page = pagedCategories.Page,
            PageSize = pagedCategories.PageSize,
            TotalCount = pagedCategories.TotalCount
        };
    }

    public async Task DeactivateCategory(int categoryId)
    {
        var category = await repository.GetByIdAsync(categoryId);
        
        if (!Category.CategoryExists(category, notificationContext)) return;

        if (category is {IsActive: false})
        {
            notificationContext.AddNotification("Desativar", "Categoria já está inativa.");
            return;
        }
        
        category!.Deactivate();
        
        repository.Update(category);
        await unitOfWork.CommitAsync();
    }

    public async Task<Category?> GetCategoryById(int categoryId)
    {
        return await repository.GetByIdAsync(categoryId);
    }
}