using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class TagUseCase(
    ITagRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
    
) : ITagUseCase
{
    public async Task<CreateResponse> CreateTag(CreateTagRequest request)
    {
        var tag = new Tag(request.Name, request.ColorHex);

        if (tag.IsInvalid)
        {
            notificationContext.AddNotifications(tag.Notifications);
            return new CreateResponse { Id = 0 };
        }
        
        await repository.CreateAsync(tag);
        await unitOfWork.CommitAsync();
        
        return new CreateResponse { Id = tag.Id };
    }

    public async Task<PagedResponse<GetSearchTagResponse>> SearchTags(SearchTagRequest request)
    {
        var pagedTags = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchTagResponse>
        {
            Items = pagedTags.Items.Select(tag => new GetSearchTagResponse
            {
                Id = tag.Id,
                Name = tag.Name,
                ColorHex = tag.HexColor,
                IsActive =  tag.IsActive
            }),
            Page =  pagedTags.Page,
            PageSize = pagedTags.PageSize,
            TotalCount = pagedTags.TotalCount
        };
    }

    public async Task DeactivateTag(int tagId)
    {
        var tag = await repository.GetByIdAsync(tagId);

        if (Tag.NotExists(tag, notificationContext)) return;

        if (tag is {IsActive: false})
        {
            notificationContext.AddNotification("Desativar", "A tag já está inativa.");
            return;
        }
        
        tag!.Deactivate();
        
        repository.Update(tag);
        await unitOfWork.CommitAsync();
    }

    public async Task<Tag?> GetTagById(int tagId)
    {
        return await repository.GetByIdAsync(tagId);
    }

    public async Task<GetTagByIdResponse> GetTagByIdResponse(int tagId)
    {
        var tag = await GetTagById(tagId);

        if (Tag.NotExists(tag, notificationContext)) return new GetTagByIdResponse();

        return new GetTagByIdResponse
        {
            Id = tag!.Id,
            Name = tag.Name,
            ColorHex = tag.HexColor,
            IsActive = tag.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateTag(int id, UpdateTagRequest request)
    {
        var tag = await GetTagById(id);

        if (Tag.NotExists(tag, notificationContext)) return new UpdateResponse {Id = 0};
        
        tag!.Update(request.Name, request.IsActive, request.ColorHex);

        if (tag.IsInvalid)
        {
            notificationContext.AddNotifications(tag.Notifications);
            return new UpdateResponse { Id = 0 };
        }
        
        repository.Update(tag);
        await unitOfWork.CommitAsync();
        return new UpdateResponse {Id = tag.Id};
    }

    public async Task<IEnumerable<Tag>> GetTagByIds(IList<int> tagIds)
    {
        return await repository.FindAsync(tag => tagIds.Contains(tag.Id));
    }
}