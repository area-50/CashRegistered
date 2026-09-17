namespace Shared.Financial.Request;

public class UpdateFinancialConfigurationRequest
{
    public decimal ApprovalThresholdAmount { get; set; }
    public bool EnableApprovalWorkflow { get; set; }
    public bool AllowAutoApprovalForManagers { get; set; }
    public decimal DefaultInterestDailyRate { get; set; }
    public decimal DefaultFineRate { get; set; }
}
