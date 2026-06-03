using Domain.Inventory.Repositories;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.Interfaces;

public interface IUomConversionUseCase
{
    Task<CreateResponse> CreateUomConversion(CreateUomConversionRequest request);
    
    Task<PagedResponse<GetSearchUomConversionResponse>> SearchUomConversion(SearchUomConversionRequest request);
    
    Task<GetUomConversionByIdResponse> GetUomConversionById(int uomId);

    Task UpdateUomConversion(int id, UpdateUomConversionRequest request);

    Task DeactivateUomConversion(int uomId);
}