using Domain.Inventory.Entities;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IUnitOfMeasureUseCase
{
    Task<CreateResponse> CreateUnitOfMeasure(CreateUnitOfMeasureRequest request);

    Task<PagedResponse<GetSearchUnitsResponse>> SearchUnits(SearchUnitOfMeasureRequest request);
    
    Task DeactivateUnitOfMeasure(int uomId);
    
    Task<UnitOfMeasure?> GetUomById(int uomId);
    
    Task<GetUnitOfMeasureByIdResponse> GetUnitOfMeasureById(int uomId);
    
    Task UpdateUnitOfMeasure(int id, UpdateUnitOfMeasureRequest request);
}