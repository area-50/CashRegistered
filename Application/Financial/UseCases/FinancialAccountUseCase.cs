using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;

namespace Application.Financial.UseCases;

public class FinancialAccountUseCase(
    IFinancialAccountRepository repository,
    NotificationContext notificationContext,
    IUnitOfWork unitOfWork
) : IFinancialAccountUseCase
{
    public async Task<CreateResponse> CreateFinancialAccount(CreateFinancialAccountRequest request)
    {
        if (await repository.ExistsByNameAsync(request.Name))
        {
            notificationContext.AddNotification("Nome", "Já existe uma conta financeira com este nome.");
            return new CreateResponse { Id = 0 };
        }

        var account = new FinancialAccount(
            name: request.Name,
            accountType: request.AccountType,
            chartOfAccountsId: request.ChartOfAccountsId,
            bankCode: request.BankCode,
            bankName: request.BankName,
            agencyNumber: request.AgencyNumber,
            accountNumber: request.AccountNumber,
            initialBalance: request.InitialBalance,
            currency: request.Currency
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

    public async Task<PagedResponse<GetSearchFinancialAccountResponse>> SearchFinancialAccounts(SearchFinancialAccountRequest request)
    {
        var pagedAccounts = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchFinancialAccountResponse>
        {
            Items = pagedAccounts.Items.Select(a => new GetSearchFinancialAccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType,
                AccountTypeName = a.AccountType.ToString(),
                ChartOfAccountsId = a.ChartOfAccountsId,
                ChartOfAccountsName = a.ChartOfAccounts?.Name ?? string.Empty,
                BankCode = a.BankCode,
                BankName = a.BankName,
                AgencyNumber = a.AgencyNumber,
                AccountNumber = a.AccountNumber,
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsActive = a.IsActive
            }),
            Page = pagedAccounts.Page,
            PageSize = pagedAccounts.PageSize,
            TotalCount = pagedAccounts.TotalCount
        };
    }

    public async Task<GetFinancialAccountByIdResponse> GetFinancialAccountById(int id)
    {
        var account = await repository.GetByIdAsync(id);

        if (FinancialAccount.NotExists(account, notificationContext)) return new GetFinancialAccountByIdResponse();

        return new GetFinancialAccountByIdResponse
        {
            Id = account!.Id,
            Name = account.Name,
            AccountType = account.AccountType,
            ChartOfAccountsId = account.ChartOfAccountsId,
            BankCode = account.BankCode,
            BankName = account.BankName,
            AgencyNumber = account.AgencyNumber,
            AccountNumber = account.AccountNumber,
            CurrentBalance = account.CurrentBalance,
            Currency = account.Currency,
            IsActive = account.IsActive
        };
    }

    public async Task<UpdateResponse> UpdateFinancialAccount(int id, UpdateFinancialAccountRequest request)
    {
        var account = await repository.GetByIdAsync(id);

        if (FinancialAccount.NotExists(account, notificationContext)) return new UpdateResponse { Id = 0 };

        if (await repository.ExistsByNameAsync(request.Name, ignoreId: id))
        {
            notificationContext.AddNotification("Nome", "Já existe uma conta financeira com este nome.");
            return new UpdateResponse { Id = 0 };
        }

        account!.Update(
            name: request.Name,
            accountType: request.AccountType,
            chartOfAccountsId: request.ChartOfAccountsId,
            bankCode: request.BankCode,
            bankName: request.BankName,
            agencyNumber: request.AgencyNumber,
            accountNumber: request.AccountNumber,
            currency: request.Currency
        );

        if (request.IsActive && !account.IsActive)
            account.Activate();
        else if (!request.IsActive && account.IsActive)
            account.Deactivate();

        if (account.IsInvalid)
        {
            notificationContext.AddNotifications(account.Notifications);
            return new UpdateResponse { Id = 0 };
        }

        repository.Update(account);
        await unitOfWork.CommitAsync();

        return new UpdateResponse { Id = account.Id };
    }

    public async Task DeactivateFinancialAccount(int id)
    {
        var account = await repository.GetByIdAsync(id);

        if (FinancialAccount.NotExists(account, notificationContext)) return;

        if (!account!.IsActive)
        {
            notificationContext.AddNotification("ContaBancaria", "A conta financeira já está desativada.");
            return;
        }

        account.Deactivate();

        repository.Update(account);
        await unitOfWork.CommitAsync();
    }
}
