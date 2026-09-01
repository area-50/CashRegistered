using Domain.Inventory.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Response;

namespace Domain.Inventory.Repositories;

public interface IUnitOfMeasureRepository : IRepository<UnitOfMeasure>
{
    Task<PagedResponse<UnitOfMeasure>> SearchAsync(SearchUnitOfMeasureRequest request);
}