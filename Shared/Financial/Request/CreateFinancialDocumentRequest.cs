using Domain.Financial.Enums;

namespace Shared.Financial.Request;

public class CreateFinancialDocumentRequest
{
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public int PersonId { get; set; }
    public int ChartOfAccountsId { get; set; }
    public int? CostCenterId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int InstallmentsCount { get; set; } = 1; // 1x, 2x, 3x...
    public decimal DiscountAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal FineAmount { get; set; }
    public decimal FineRate { get; set; }
    public decimal InterestDailyRate { get; set; }
    public string? Notes { get; set; }
}
