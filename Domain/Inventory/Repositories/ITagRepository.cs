using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<PagedResponse<Tag>> SearchAsync(SearchTagRequest request);
}