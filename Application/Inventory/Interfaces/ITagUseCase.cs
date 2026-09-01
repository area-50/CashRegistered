using Domain.Inventory.Entities;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ITagUseCase
{
    Task<CreateResponse> CreateTag(CreateTagRequest request);
    
    Task<PagedResponse<GetSearchTagResponse>> SearchTags(SearchTagRequest request);
    
    Task DeactivateTag(int tagId);
    
    Task<Tag?> GetTagById(int tagId);

    Task<GetTagByIdResponse> GetTagByIdResponse(int tagId);

    Task<UpdateResponse> UpdateTag(int id, UpdateTagRequest request);

    Task<IEnumerable<Tag>> GetTagByIds(IList<int> tagIds);
}