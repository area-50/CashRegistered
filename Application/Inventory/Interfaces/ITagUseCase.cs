using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface ITagUseCase
{
    Task<CreateResponse> CreateTag(CreateTagRequest request);
    
    Task<PagedResponse<GetSearchTagResponse>> SearchTags(SearchTagRequest request);
    
    Task DeactivateTag(int tagId);
    
    Task<IEnumerable<Tag>> GetTagByIds(IList<int> tagIds);
}