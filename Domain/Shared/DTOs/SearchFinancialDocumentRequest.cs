using Domain.Financial.Enums;
using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchFinancialDocumentRequest : PagedRequest
{
    public DocumentType? DocumentType { get; set; }
    public int? PersonId { get; set; }
    public int? ChartOfAccountsId { get; set; }
    public int? CostCenterId { get; set; }
    public DocumentStatus? Status { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DateType { get; set; } // "DueDate", "IssueDate", "PaymentDate"
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
