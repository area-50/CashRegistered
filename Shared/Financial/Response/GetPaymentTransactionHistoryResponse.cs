using Domain.Financial.Enums;

namespace Shared.Financial.Response;

public class GetPaymentTransactionHistoryResponse
{
    public int Id { get; set; }
    public int FinancialInstallmentId { get; set; }
    public int InstallmentNumber { get; set; }
    public int FinancialAccountId { get; set; }
    public string FinancialAccountName { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal InterestApplied { get; set; }
    public decimal FineApplied { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public string? TransactionReceiptNumber { get; set; }
    public string? Notes { get; set; }
    public string CreatedByUserName { get; set; } = null!;
}
