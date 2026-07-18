using Application.Identity.Interfaces;
using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class CostCenterUseCase(
    ICostCenterRepository repository,
    IUserUseCase userUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : ICostCenterUseCase
{
    public async Task<CreateResponse> CreateCostCenter(CreateCostCenterRequest request)
    {
        var manager = await userUseCase.GetUserById(request.ManagerId);
        if (manager == null)
        {
            notificationContext.AddNotification("ManagerId", "O usuário gerente não foi encontrado.");
            return new CreateResponse();
        }

        var costCenter = new CostCenter(request.Name, request.ManagerId);
        
        await repository.CreateAsync(costCenter);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = costCenter.Id };
    }

    public async Task UpdateCostCenter(int id, UpdateCostCenterRequest request)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (costCenter == null)
        {
            notificationContext.AddNotification("CostCenter", "O centro de custo não existe.");
            return;
        }

        var manager = await userUseCase.GetUserById(request.ManagerId);
        if (manager == null)
        {
            notificationContext.AddNotification("ManagerId", "O usuário gerente não foi encontrado.");
            return;
        }

        costCenter.Name = request.Name;
        costCenter.ManagerId = request.ManagerId;

        if (request.IsActive)
            costCenter.Activate();
        else
            costCenter.Deactivate();

        repository.Update(costCenter);
        await unitOfWork.CommitAsync();
    }

    public async Task DeactivateCostCenter(int id)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (costCenter == null)
        {
            notificationContext.AddNotification("CostCenter", "O centro de custo não existe.");
            return;
        }

        costCenter.Deactivate();

        repository.Update(costCenter);
        await unitOfWork.CommitAsync();
    }

    public async Task<GetCostCenterByIdResponse?> GetCostCenterById(int id)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (costCenter == null)
        {
            return null;
        }

        return new GetCostCenterByIdResponse
        {
            Id = costCenter.Id,
            Name = costCenter.Name,
            ManagerId = costCenter.ManagerId,
            ManagerName = costCenter.Manager?.Person?.Name?.FirstName + " " + costCenter.Manager?.Person?.Name?.LastName,
            IsActive = costCenter.IsActive
        };
    }

    public async Task<PagedResponse<GetSearchCostCenterResponse>> SearchCostCenters(SearchCostCenterRequest request)
    {
        var pagedCostCenters = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchCostCenterResponse>
        {
            Items = pagedCostCenters.Items.Select(c => new GetSearchCostCenterResponse
            {
                Id = c.Id,
                Name = c.Name,
                ManagerName = c.Manager?.Person?.Name?.FirstName + " " + c.Manager?.Person?.Name?.LastName,
                IsActive = c.IsActive
            }),
            TotalCount = pagedCostCenters.TotalCount,
            Page = pagedCostCenters.Page,
            PageSize = pagedCostCenters.PageSize
        };
    }
}
