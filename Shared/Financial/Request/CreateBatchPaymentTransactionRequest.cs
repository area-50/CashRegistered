using Domain.Financial.Enums;

namespace Shared.Financial.Request;

public class CreateBatchPaymentTransactionRequest
{
    public int FinancialAccountId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReceiptNumber { get; set; }
    public string? Notes { get; set; }
    public List<PaymentTransactionItemRequest> Items { get; set; } = new();
}
