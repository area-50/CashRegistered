namespace Shared.Financial.Request;

public class CreateExpenseRequest
{
    public int UserId { get; set; }

    public int CashFlowId { get; set; }
    
    public decimal ExpenseValue { get; set; }

    public string ExpenseDescription { get; set; } = null!;
}