using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class UnitOfMeasureUseCase(
    IUnitOfMeasureRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext) : IUnitOfMeasureUseCase
{
    public async Task<CreateResponse> CreateUnitOfMeasure(CreateUnitOfMeasureRequest request)
    {
        var uom = new UnitOfMeasure(
            request.Code,
            request.Name,
            request.AllowDecimals
        );  

       await uom.CodeExists(repository, request.Code);
        
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
    
    public async Task<PagedResponse<GetSearchUnitsResponse>> SearchUnits(SearchUnitOfMeasureRequest request)
    {
        var pagedUoms = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchUnitsResponse>
        {
            Items = pagedUoms.Items.Select(u => new GetSearchUnitsResponse
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                AllowDecimals = u.AllowDecimals,
                IsActive = u.IsActive
            }),
            Page = pagedUoms.Page,
            PageSize = pagedUoms.PageSize,
            TotalCount = pagedUoms.TotalCount
        };
    }

    public async Task DeactivateUnitOfMeasure(int uomId)
    {
        var uom = await GetUomById(uomId);

        if (UnitOfMeasure.NotExists(uom, notificationContext)) return;

        if (uom is { IsActive: false })
        {
            notificationContext.AddNotification("Desativar", "Unidade de Medida já está inativa.");
            return;
        }
        
        uom!.Deactivate();
        
        repository.Update(uom);
        await unitOfWork.CommitAsync();
    }

    public async Task<UnitOfMeasure?> GetUomById(int uomId)
    {
        return await repository.GetByIdAsync(uomId);
    }

    public async Task<GetUnitOfMeasureByIdResponse> GetUnitOfMeasureById(int uomId)
    {
        var uom = await GetUomById(uomId);

        if (UnitOfMeasure.NotExists(uom, notificationContext)) return new GetUnitOfMeasureByIdResponse();

        return new GetUnitOfMeasureByIdResponse
        {
            Id = uom!.Id,
            Code = uom.Code,
            Name = uom.Name,
            AllowDecimals = uom.AllowDecimals,
            IsActive = uom.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateUnitOfMeasure(int id, UpdateUnitOfMeasureRequest request)
    {
        var uom = await GetUomById(id);

        if (UnitOfMeasure.NotExists(uom, notificationContext)) return new UpdateResponse {Id = 0};
        
        uom!.Update(
            request.Code,
            request.Name,
            request.IsActive,
            request.AllowDecimals
        );
        
        repository.Update(uom);
        await unitOfWork.CommitAsync();
        return new UpdateResponse {Id = uom.Id};
    }
}