namespace Shared.Financial.Request;

public class UpdateFinancialDocumentRequest
{
    public string DocumentNumber { get; set; } = null!;
    public int PersonId { get; set; }
    public int ChartOfAccountsId { get; set; }
    public int? CostCenterId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
