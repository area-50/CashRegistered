using Domain.Inventory.Entities;
using Domain.Shared.DTOs;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IUnitOfMeasureUseCase
{
    Task<CreateResponse> CreateUnitOfMeasure(CreateUnitOfMeasureRequest request);

    Task<PagedResponse<GetSearchUnitsResponse>> SearchUnits(SearchUnitOfMeasureRequest request);
    
    Task DeactivateUnitOfMeasure(int uomId);
    
    Task<UnitOfMeasure?> GetUomById(int uomId);
    
    Task<GetUnitOfMeasureByIdResponse> GetUnitOfMeasureById(int uomId);
    
    Task<UpdateResponse> UpdateUnitOfMeasure(int id, UpdateUnitOfMeasureRequest request);
}