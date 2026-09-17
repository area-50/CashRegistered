using Domain.Financial.Enums;

namespace Shared.Financial.Response;

public class GetFinancialDocumentByIdResponse
{
    public int Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public int PersonId { get; set; }
    public string PersonName { get; set; } = null!;
    public int ChartOfAccountsId { get; set; }
    public string ChartOfAccountsName { get; set; } = null!;
    public int? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal FineAmount { get; set; }
    public decimal FineRate { get; set; }
    public decimal InterestDailyRate { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DocumentStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public List<FinancialInstallmentResponse> Installments { get; set; } = new();
}

public class FinancialInstallmentResponse
{
    public int Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal FineAmount { get; set; }
    public InstallmentStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
}
