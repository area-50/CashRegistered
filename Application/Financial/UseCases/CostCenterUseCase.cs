using Application.Identity.Interfaces;
using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Domain.Shared.Notifications;
using Domain.Shared.Response;

namespace Application.Financial.UseCases;

public class CostCenterUseCase(
    ICostCenterRepository repository,
    IUserUseCase userUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : ICostCenterUseCase
{
    public async Task<CreateResponse> CreateCostCenter(CreateCostCenterRequest request)
    {
        if (await repository.ExistsByNameAsync(request.Name))
        {
            notificationContext.AddNotification("Nome", "Já existe um centro de custo com este nome.");
            return new CreateResponse { Id = 0 };
        }

        var manager = await userUseCase.GetUserById(request.ManagerId);
        if (manager == null)
        {
            notificationContext.AddNotification("GerenteId", "O usuário gerente não foi encontrado.");
            return new CreateResponse { Id = 0 };
        }

        var costCenter = new CostCenter(request.Name, request.ManagerId);

        if (costCenter.IsInvalid)
        {
            notificationContext.AddNotifications(costCenter.Notifications);
            return new CreateResponse { Id = 0 };
        }
        
        await repository.CreateAsync(costCenter);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = costCenter.Id };
    }

    public async Task<UpdateResponse> UpdateCostCenter(int id, UpdateCostCenterRequest request)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (CostCenter.NotExists(costCenter, notificationContext))
        {
            return new UpdateResponse { Id = 0 };
        }

        if (await repository.ExistsByNameAsync(request.Name, ignoreId: id))
        {
            notificationContext.AddNotification("Nome", "Já existe um centro de custo com este nome.");
            return new UpdateResponse { Id = 0 };
        }

        var manager = await userUseCase.GetUserById(request.ManagerId);
        if (manager == null)
        {
            notificationContext.AddNotification("GerenteId", "O usuário gerente não foi encontrado.");
            return new UpdateResponse { Id = 0 };
        }

        costCenter!.Update(request.Name, request.ManagerId);

        if (request.IsActive && !costCenter.IsActive)
            costCenter.Activate();
        else if (!request.IsActive && costCenter.IsActive)
            costCenter.Deactivate();

        if (costCenter.IsInvalid)
        {
            notificationContext.AddNotifications(costCenter.Notifications);
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(costCenter);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = costCenter.Id };
    }

    public async Task DeactivateCostCenter(int id)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (CostCenter.NotExists(costCenter, notificationContext))
        {
            return;
        }

        if (!costCenter!.IsActive)
        {
            notificationContext.AddNotification("CentroDeCusto", "O centro de custo já está desativado.");
            return;
        }

        costCenter.Deactivate();

        repository.Update(costCenter);
        await unitOfWork.CommitAsync();
    }

    public async Task<GetCostCenterByIdResponse?> GetCostCenterById(int id)
    {
        var costCenter = await repository.GetByIdAsync(id);
        if (CostCenter.NotExists(costCenter, notificationContext))
        {
            return null;
        }

        var managerName = string.Join(" ", new[] { costCenter!.Manager?.Person?.Name?.FirstName, costCenter.Manager?.Person?.Name?.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

        return new GetCostCenterByIdResponse
        {
            Id = costCenter.Id,
            Name = costCenter.Name,
            ManagerId = costCenter.ManagerId,
            ManagerName = managerName,
            IsActive = costCenter.IsActive
        };
    }

    public async Task<PagedResponse<GetSearchCostCenterResponse>> SearchCostCenters(SearchCostCenterRequest request)
    {
        var pagedCostCenters = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchCostCenterResponse>
        {
            Items = pagedCostCenters.Items.Select(c =>
            {
                var managerName = string.Join(" ", new[] { c.Manager?.Person?.Name?.FirstName, c.Manager?.Person?.Name?.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

                return new GetSearchCostCenterResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ManagerName = managerName,
                    IsActive = c.IsActive
                };
            }),
            TotalCount = pagedCostCenters.TotalCount,
            Page = pagedCostCenters.Page,
            PageSize = pagedCostCenters.PageSize
        };
    }
}
