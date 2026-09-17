namespace Shared.Financial.Response;

public class GetFinancialConfigurationResponse
{
    public int Id { get; set; }
    public decimal ApprovalThresholdAmount { get; set; }
    public bool EnableApprovalWorkflow { get; set; }
    public bool AllowAutoApprovalForManagers { get; set; }
    public decimal DefaultInterestDailyRate { get; set; }
    public decimal DefaultFineRate { get; set; }
    public bool IsActive { get; set; }
}
