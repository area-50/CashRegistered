using Domain.Shared.DTOs;

namespace Application.Financial.Interfaces;

public interface IExpenseUseCase
{
    public Task CreateExpense(CreateExpenseRequest request);
}