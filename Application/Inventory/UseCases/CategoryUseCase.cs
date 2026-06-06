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
        
        if (Category.NotExists(category, notificationContext)) return;

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

    public async Task<GetCategoryByIdResponse> GetCategoryByIdResponse(int categoryId)
    {
        var category = await GetCategoryById(categoryId);

        if (Category.NotExists(category, notificationContext)) return new GetCategoryByIdResponse();

        return new GetCategoryByIdResponse
        {
            Id = category!.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        var category = await GetCategoryById(id);

        if (Category.NotExists(category, notificationContext)) return new UpdateResponse { Id = 0 };

        category!.Update(request.Name, request.ParentCategoryId, request.IsActive);

        if (category.IsInvalid)
        {
            notificationContext.AddNotifications(category.Notifications);
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(category);
        await unitOfWork.CommitAsync();
        return new UpdateResponse { Id = category.Id };
    }
}