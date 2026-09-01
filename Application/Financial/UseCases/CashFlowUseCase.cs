using Application.Identity.Interfaces;
using Application.Financial.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Shared.Financial.Response;
using Domain.Shared.Validations;

namespace Application.Financial.UseCases;

public class CashFlowUseCase(
    IUserUseCase userUseCase,
    ICashFlowRepository repository,
    IUnitOfWork unitOfWork
) : GeneralValidator, ICashFlowUseCase
{
    public async Task CreateCashFlow(CreateCashFlowRequest request)
    {
        var targetUser = await userUseCase.GetValidUserById(request.UserId);
        
        var cashFlow = new CashFlow(request.UserId);
        
        cashFlow.ValidateCashFlowLinkedToUser(targetUser);
        
        await repository.CreateAsync(cashFlow);
        
        await unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<GetCashFlowsAvailableResponse>> GetCashFlowsAvailable()
    {
        var result = await repository.FindAsync(cashFlow => true);

        var response = result.Select(c => new GetCashFlowsAvailableResponse()
            {
                Id = c.Id,
                UserId =  c.UserId,
                UserName = c.User!.Person.Name,
                CurrentBalance = c.CurrentBalance
            }
        );
        return response;
    }

    public async Task<CashFlow?> GetCashFlowById(int cashFlowId)
    {
        return await repository.GetByIdAsync(cashFlowId);
    }

    public void UpdateCashFlow(CashFlow cashFlow)
    {
        repository.Update(cashFlow);
    }

    public async Task<IEnumerable<GetExpensesByCashFlowIdResponse>> GetExpensesByCashFlowId(int cashFlowId)
    {
        var result = await repository.FindAsync(
            c => c.Id == cashFlowId
        );

        var response = result
            .Select(c => new GetExpensesByCashFlowIdResponse
                {
                    CashFlowId = c.Id,
                    UserId = c.UserId,
                    UserName = c.User!.Person.Name,
                    ExpenseValues = c.Expenses?.Select<Expense, ExpenseValues>(e =>
                        new ExpenseValues
                        {
                            ExpenseDescription = e.ExpenseDescription,
                            Value = e.ExpenseValue
                        }
                    ).ToArray()
                }
            );
        return response;
    }

    public async Task AddCash(AddCashRequest request)
    {
        var cashFlow = await GetCashFlowById(request.CashFlowId);
        
        CashFlow.ValidateCashFlowExists(cashFlow);
        
        cashFlow!.IncreaseCurrentBalance(request.Value);
        
        repository.Update(cashFlow);

        await unitOfWork.CommitAsync();
    }
}