using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Request;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class UomConversionUseCase(
    IUomConversionRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : IUomConversionUseCase
{
    public async Task<CreateResponse> CreateUomConversion(CreateUomConversionRequest request)
    {
        var uom = new UomConversion(
            request.FromUomId,
            request.ToUomId,
            request.Multiplier,
            request.ProductId
        );

        if (uom.IsInvalid)
        {
            notificationContext.AddNotifications(uom.Notifications);
            return new CreateResponse
            {
                Id = 0
            };
        }

        await repository.CreateAsync(uom);
        await unitOfWork.CommitAsync();
        
        return new CreateResponse
        {
            Id = uom.Id
        };
    }

    public async Task<PagedResponse<GetSearchUomConversionResponse>> SearchUomConversion(
        SearchUomConversionRequest request
    )
    {
        var pagedUomSearches = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchUomConversionResponse>
        {
            Items = pagedUomSearches.Items.Select(uom => new GetSearchUomConversionResponse
                {
                    Id = uom.Id,
                    FromUnitName = uom.FromUom.Name,
                    FromUnitSymbol = uom.FromUom.Code,
                    FromUnitSymbolId = uom.FromUom.Id,
                    ToUnitName = uom.ToUom.Name,
                    ToUnitSymbol = uom.ToUom.Code,
                    ToUnitSymbolId = uom.ToUom.Id,
                    Multiplier = uom.Multiplier,
                    ProductName = uom.Product?.Name,
                    ProductId = uom.ProductId,
                    IsActive = uom.IsActive
                }
            ),
            Page = pagedUomSearches.Page,
            PageSize = pagedUomSearches.PageSize,
            TotalCount = pagedUomSearches.TotalCount
        };
    }

    public async Task<GetUomConversionByIdResponse> GetUomConversionById(int uomId)
    {
        var uom = await repository.GetByIdAsync(uomId);

        if (!UomConversion.UomConversionExists(uom, notificationContext)) return new GetUomConversionByIdResponse();

        return new GetUomConversionByIdResponse
        {
            Id = uom!.Id,
            FromUomId = uom.FromUomId,
            ToUomId = uom.ToUomId,
            Multiplier = uom.Multiplier,
            ProductId = uom.ProductId,
            IsActive = uom.IsActive
        };
    }

    public async Task UpdateUomConversion(int id, UpdateUomConversionRequest request)
    {
        var uom = await repository.GetByIdAsync(id);

        if (!UomConversion.UomConversionExists(uom, notificationContext)) return;

        uom!.UpdateUomConversion(
            request.FromUomId,
            request.ToUomId,
            request.Multiplier,
            request.IsActive,
            request.ProductId
        );

        repository.Update(uom);
        await unitOfWork.CommitAsync();
    }

    public async Task DeactivateUomConversion(int uomId)
    {
        var uom = await repository.GetByIdAsync(uomId);

        if (!UomConversion.UomConversionExists(uom, notificationContext)) return;
        
        if (uom is { IsActive: false })
        {
            notificationContext.AddNotification("Desativar", "Essa conversão já está inativa.");
            return;
        }
        
        uom!.Deactivate();

        repository.Update(uom);
        await unitOfWork.CommitAsync();
    }
}