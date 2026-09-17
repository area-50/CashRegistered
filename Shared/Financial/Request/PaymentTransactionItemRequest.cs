namespace Shared.Financial.Request;

public class PaymentTransactionItemRequest
{
    public int FinancialInstallmentId { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal InterestApplied { get; set; }
    public decimal FineApplied { get; set; }
}
