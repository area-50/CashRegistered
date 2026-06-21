using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;

using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class WarehouseUseCase(
    IWarehouseRepository repository,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IWarehouseUseCase
{
    public async Task<CreateResponse> CreateWarehouse(CreateWarehouseRequest request)
    {
        
        var warehouse = new Warehouse(
            request.Name,
            request.Type
        );

        if (warehouse.IsInvalid)
        {
            notificationContext.AddNotifications(warehouse.Notifications);
            return new CreateResponse { Id = 0 };
        }
        
        await repository.CreateAsync(warehouse);
        await unitOfWork.CommitAsync();
        
        return new CreateResponse { Id = warehouse.Id };
    }

    public async Task<PagedResponse<GetWarehouseByIdResponse>> SearchWarehouses(SearchWarehouseRequest request)
    {
        var pagedWarehouses = await repository.SearchAsync(request);

        return new PagedResponse<GetWarehouseByIdResponse>
        {
            Items = pagedWarehouses.Items.Select(w =>
                new GetWarehouseByIdResponse
            {
                Id = w.Id,
                Name = w.Name,
                Type = w.Type,
                IsActive = w.IsActive
            }
            ),
            Page = pagedWarehouses.Page,
            PageSize = pagedWarehouses.PageSize,
            TotalCount =  pagedWarehouses.TotalCount
        };
    }

    public async Task<Warehouse?> GetById(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Warehouse>> ListAll()
    {
        return await repository.FindAsync(_ => true);
    }

    public async Task<GetWarehouseByIdResponse> GetWarehouseById(int id)
    {
        var warehouse = await repository.GetByIdAsync(id);

        if (Warehouse.NotExists(warehouse, notificationContext)) return new GetWarehouseByIdResponse();

        return new GetWarehouseByIdResponse
        {
            Id = warehouse!.Id,
            Name = warehouse.Name,
            Type = warehouse.Type,
            IsActive = warehouse.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateWarehouse(int id, UpdateWarehouseRequest request)
    {
        var warehouse = await repository.GetByIdAsync(id);

        if (Warehouse.NotExists(warehouse, notificationContext)) return new UpdateResponse { Id = 0 };

        warehouse!.Update(
            request.Name,
            request.Type,
            request.IsActive
        );
        
        if (warehouse.IsInvalid)
        {
            notificationContext.AddNotifications(warehouse.Notifications);
            return new UpdateResponse { Id = 0 };
        }
        
        repository.Update(warehouse);
        await unitOfWork.CommitAsync();
        return new UpdateResponse { Id = warehouse.Id };
    }

    public async Task DeactivateWarehouse(int id)
    {
        var warehouse = await GetById(id);

        if (Warehouse.NotExists(warehouse, notificationContext)) return;

        if (!warehouse!.IsActive)
        {
            notificationContext.AddNotification("Almoxarifado", "Almoxarifado já desativado");
            return;
        }

        warehouse.Deactivate();

        repository.Update(warehouse);
        await unitOfWork.CommitAsync();
    }
    }

