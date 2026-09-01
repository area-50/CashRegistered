using Domain.Identity.Validations;
using Domain.Financial.Validations;
using Domain.Validations;
using Domain.Shared.Abstractions;
using Domain.Shared.Exceptions;

namespace Domain.Financial.Entities;

public class Expense : BaseEntity
{
    public Expense(string expenseDescription, decimal expenseValue, int cashFlowId)
    {
        ExpenseDescription = expenseDescription;
        ExpenseValue = expenseValue;
        CashFlowId = cashFlowId;
        
        Validate(
            this,
            new ExpenseValidation()!,
            error => new DomainException(error)
        );
    }

    protected Expense() {}
    
    public string ExpenseDescription { get; private set; }

    public decimal ExpenseValue { get; private set; }

    public int CashFlowId { get; init; }

    public CashFlow CashFlow { get; init; }
}