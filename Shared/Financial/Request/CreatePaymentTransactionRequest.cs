using Domain.Financial.Enums;

namespace Shared.Financial.Request;

public class CreatePaymentTransactionRequest
{
    public int FinancialInstallmentId { get; set; }
    
    public int FinancialAccountId { get; set; }
    
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    public decimal AmountPaid { get; set; }
    
    public decimal DiscountApplied { get; set; }
    
    public decimal InterestApplied { get; set; }
    
    public decimal FineApplied { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    
    public string? TransactionReceiptNumber { get; set; }
    
    public string? Notes { get; set; }
}
