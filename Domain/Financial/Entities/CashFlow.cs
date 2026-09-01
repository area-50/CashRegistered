using Domain.Identity.Validations;
using Domain.Financial.Validations;
using Domain.Validations;
using Domain.Identity.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.Exceptions;

namespace Domain.Financial.Entities;

public class CashFlow : BaseEntity
{
    public CashFlow(int userId)
    {
        ValidateUserId(userId);
    }

    protected CashFlow() {}

    public decimal? CurrentBalance { get; private set; } = 0;
    
    public int UserId { get; private set; }
    
    public User? User { get; init; }

    public IReadOnlyCollection<Expense>? Expenses { get; init; }

    private void ValidateUserId(int userId)
    {
        UserId = userId;
        Validate(
            this,
            new ValidateUserIdValidation()!,
            error => new DomainException(error)
        );
    }

    public void DecreaseCurrentBalance(decimal? newValue)
    {
        ValidateUpdateCurrentBalance(newValue);
        CurrentBalance -= newValue;
        RegisterUpdate();
    }

    public void IncreaseCurrentBalance(decimal? newValue)
    {
        ValidateUpdateCurrentBalance(newValue);
        CurrentBalance += newValue;
        RegisterUpdate();
    }

    public void ValidateCashFlowLinkedToUser(User? user)
    {
        if (user?.CashFlow == null) return;

        Id = user.CashFlow.Id;
            
        Validate(
            this,
            new CashFlowLinkedToUserValidation()!,
            error => new DomainException(error)
        );
    }

    public static void ValidateCashFlowExists(CashFlow? targetCashFlow)
    {
        Validate(
            targetCashFlow,
            new NullableValidation<CashFlow>(),
            error => new DomainException(error),
            ["O fluxo de caixa informado não existe."]
        );
    }

    private static void ValidateUpdateCurrentBalance(decimal? newValue)
    {
        switch (newValue)
        {
            case null:
                throw new DomainException(
                    message: "Um ou mais erros de validação de domínio ocorreram.",
                    errors:["O valor não pode ser nulo."]    
                );
            case <= 0:
                throw new DomainException(
                    message: "Um ou mais erros de validação de domínio ocorreram.",
                    errors:["O valor dever se maior que zero"]
                );
        }
    }

    public static bool NotExists(CashFlow? cashFlow, NotificationContext notificationContext)
    {
        if (cashFlow != null) return false;
        notificationContext.AddNotification("FluxoDeCaixa", "O fluxo de caixa informado não existe.");
        return true;
    }
}