using Domain.Financial.Enums;

namespace Shared.Financial.Response;

public class GetSearchFinancialDocumentResponse
{
    public int Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = null!;
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
    public decimal PaidAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal FineRate { get; set; }
    public decimal InterestDailyRate { get; set; }
    public DocumentStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int InstallmentsCount { get; set; }
    public bool IsActive { get; set; }
}
