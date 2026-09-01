using Application.Identity.Interfaces;
using Application.Financial.Interfaces;
using Domain.Identity.Entities;
using Domain.Financial.Entities;
using Domain.Financial.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Validations;

namespace Application.Financial.UseCases;

public class ExpenseUseCase(
    IExpenseRepository expenseRepository,
    IUserUseCase userUseCase,
    ICashFlowUseCase cashFlowUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : GeneralValidator, IExpenseUseCase
{
    public async Task CreateExpense(CreateExpenseRequest request)
    {
        var user = await userUseCase.GetUserById(request.UserId);

        User.ValidateUserExists(user, notificationContext);
        if (notificationContext.IsInvalid) return;

        user!.ValidateUserHasCashFlow();
        if (user.IsInvalid)
        {
            notificationContext.AddNotifications(user.Notifications);
            return;
        }

        var newExpense = new Expense(
            request.ExpenseDescription,
            request.ExpenseValue,
            request.CashFlowId
        );
        
        await expenseRepository.CreateAsync(newExpense);

        var cashFlow = await cashFlowUseCase.GetCashFlowById(request.CashFlowId);
        
        cashFlow!.DecreaseCurrentBalance(request.ExpenseValue);
        
        cashFlowUseCase.UpdateCashFlow(cashFlow);

        await unitOfWork.CommitAsync();
    }
}
