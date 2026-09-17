namespace Shared.Financial.Response;

public class GetFinancialSettlementPreviewResponse
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal DocumentFineRate { get; set; }
    public decimal DocumentInterestDailyRate { get; set; }
    public List<FinancialSettlementPreviewItemResponse> Items { get; set; } = new();
}

public class FinancialSettlementPreviewItemResponse
{
    public int InstallmentId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int OverdueDays { get; set; }
    public decimal DailyInterestAmount { get; set; }
    public decimal CalculatedFineAmount { get; set; }
    public decimal CalculatedInterestAmount { get; set; }
    public decimal SuggestedAmountPaid { get; set; }
}
