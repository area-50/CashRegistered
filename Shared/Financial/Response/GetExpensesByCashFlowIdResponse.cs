using Domain.Shared.ValueObjects;

namespace Shared.Financial.Response;

public class GetExpensesByCashFlowIdResponse
{
    public int CashFlowId { get; set; }

    public int UserId { get; set; }

    public Name UserName { get; set; } = null!;

    public IEnumerable<ExpenseValues>? ExpenseValues { get; set; }
}

public class ExpenseValues
{
    public string ExpenseDescription { get; set; } = null!;

    public decimal Value { get; set; }
}