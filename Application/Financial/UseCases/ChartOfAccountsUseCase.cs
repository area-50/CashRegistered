using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;

namespace Application.Financial.UseCases;

public class ChartOfAccountsUseCase(
    IChartOfAccountsRepository repository,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IChartOfAccountsUseCase
{
    public async Task<CreateResponse> CreateChartOfAccounts(CreateChartOfAccountsRequest request)
    {
        if (request.ParentAccountId is > 0)
        {
            var parent = await repository.GetByIdAsync(request.ParentAccountId.Value);
            if (parent == null)
            {
                notificationContext.AddNotification("ContaPai", "Conta pai informada não existe.");
                return new CreateResponse { Id = 0 };
            }

            if (!parent.IsSynthetic)
            {
                notificationContext.AddNotification("ContaPai", "Apenas contas sintéticas podem ser selecionadas como conta pai.");
                return new CreateResponse { Id = 0 };
            }
        }

        if (await repository.ExistsByCodeAsync(request.Code))
        {
            notificationContext.AddNotification("Codigo", "Já existe um plano de contas cadastrado com este código.");
            return new CreateResponse { Id = 0 };
        }

        if (await repository.ExistsByNameAsync(request.Name))
        {
            notificationContext.AddNotification("Nome", "Já existe um plano de contas cadastrado com este nome.");
            return new CreateResponse { Id = 0 };
        }

        var account = new ChartOfAccounts(
            code: request.Code,
            name: request.Name,
            accountType: request.AccountType,
            nature: request.Nature,
            parentAccountId: request.ParentAccountId,
            isSynthetic: request.IsSynthetic,
            allowPosting: request.AllowPosting
        );

        if (account.IsInvalid)
        {
            notificationContext.AddNotifications(account.Notifications);
            return new CreateResponse { Id = 0 };
        }

        await repository.CreateAsync(account);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = account.Id };
    }

    public async Task<PagedResponse<GetSearchChartOfAccountsResponse>> SearchChartOfAccounts(SearchChartOfAccountsRequest request)
    {
        var pagedAccounts = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchChartOfAccountsResponse>
        {
            Items = pagedAccounts.Items.Select(a => new GetSearchChartOfAccountsResponse
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                AccountType = a.AccountType,
                Nature = a.Nature,
                ParentAccountId = a.ParentAccountId,
                ParentAccountName = a.ParentAccount?.Name,
                IsSynthetic = a.IsSynthetic,
                AllowPosting = a.AllowPosting,
                IsActive = a.IsActive
            }),
            Page = pagedAccounts.Page,
            PageSize = pagedAccounts.PageSize,
            TotalCount = pagedAccounts.TotalCount

        };
    }

    public async Task<GetChartOfAccountsByIdResponse> GetChartOfAccountsById(int id)
    {
        var account = await repository.GetByIdAsync(id);

        if (ChartOfAccounts.NotExists(account, notificationContext)) return new GetChartOfAccountsByIdResponse();

        return new GetChartOfAccountsByIdResponse
        {
            Id = account!.Id,
            Code = account.Code,
            Name = account.Name,
            AccountType = account.AccountType,
            Nature = account.Nature,
            ParentAccountId = account.ParentAccountId,
            ParentAccountName = account.ParentAccount?.Name,
            IsSynthetic = account.IsSynthetic,
            AllowPosting = account.AllowPosting,
            IsActive = account.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateChartOfAccounts(int id, UpdateChartOfAccountsRequest request)
    {
        var account = await repository.GetByIdAsync(id);

        if (ChartOfAccounts.NotExists(account, notificationContext)) return new UpdateResponse { Id = 0 };

        if (request.ParentAccountId is > 0)
        {
            if (request.ParentAccountId.Value == id)
            {
                notificationContext.AddNotification("ContaPai", "Uma conta não pode ser pai dela mesma.");
                return new UpdateResponse { Id = 0 };
            }

            var parent = await repository.GetByIdAsync(request.ParentAccountId.Value);
            if (parent == null)
            {
                notificationContext.AddNotification("ContaPai", "Conta pai informada não existe.");
                return new UpdateResponse { Id = 0 };
            }

            if (!parent.IsSynthetic)
            {
                notificationContext.AddNotification("ContaPai", "Apenas contas sintéticas podem ser selecionadas como conta pai.");
                return new UpdateResponse { Id = 0 };
            }
        }

        if (await repository.ExistsByCodeAsync(request.Code, ignoreId: id))
        {
            notificationContext.AddNotification("Codigo", "Já existe um plano de contas cadastrado com este código.");
            return new UpdateResponse { Id = 0 };
        }

        if (await repository.ExistsByNameAsync(request.Name, ignoreId: id))
        {
            notificationContext.AddNotification("Nome", "Já existe um plano de contas cadastrado com este nome.");
            return new UpdateResponse { Id = 0 };
        }

        account!.Update(
            code: request.Code,
            name: request.Name,
            accountType: request.AccountType,
            nature: request.Nature,
            parentAccountId: request.ParentAccountId,
            isSynthetic: request.IsSynthetic,
            allowPosting: request.AllowPosting
        );

        if (account.IsInvalid)
        {
            notificationContext.AddNotifications(account.Notifications);
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(account);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = account.Id };
    }

    public async Task DeactivateChartOfAccounts(int id)
    {
        var account = await repository.GetByIdAsync(id);

        if (ChartOfAccounts.NotExists(account, notificationContext)) return;

        account!.Deactivate();
        repository.Update(account);
        await unitOfWork.CommitAsync();
    }
}
