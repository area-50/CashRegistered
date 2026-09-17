using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class FinancialConfiguration : BaseEntity
{
    public decimal ApprovalThresholdAmount { get; private set; }
    public bool EnableApprovalWorkflow { get; private set; }
    public bool AllowAutoApprovalForManagers { get; private set; }
    public decimal DefaultInterestDailyRate { get; private set; }
    public decimal DefaultFineRate { get; private set; }

    protected FinancialConfiguration() { }

    public FinancialConfiguration(
        decimal approvalThresholdAmount = 0m,
        bool enableApprovalWorkflow = false,
        bool allowAutoApprovalForManagers = false,
        decimal defaultInterestDailyRate = 0m,
        decimal defaultFineRate = 0m)
    {
        ApprovalThresholdAmount = approvalThresholdAmount;
        EnableApprovalWorkflow = enableApprovalWorkflow;
        AllowAutoApprovalForManagers = allowAutoApprovalForManagers;
        DefaultInterestDailyRate = defaultInterestDailyRate;
        DefaultFineRate = defaultFineRate;

        Validate();
    }

    public void UpdateSettings(
        decimal approvalThresholdAmount,
        bool enableApprovalWorkflow,
        bool allowAutoApprovalForManagers,
        decimal defaultInterestDailyRate,
        decimal defaultFineRate)
    {
        ApprovalThresholdAmount = approvalThresholdAmount;
        EnableApprovalWorkflow = enableApprovalWorkflow;
        AllowAutoApprovalForManagers = allowAutoApprovalForManagers;
        DefaultInterestDailyRate = defaultInterestDailyRate;
        DefaultFineRate = defaultFineRate;

        Validate();
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (ApprovalThresholdAmount < 0)
            AddNotification("ApprovalThresholdAmount", "O valor de limite de alçada não pode ser negativo.");

        if (DefaultInterestDailyRate < 0)
            AddNotification("DefaultInterestDailyRate", "A taxa de juros diária não pode ser negativa.");

        if (DefaultFineRate < 0)
            AddNotification("DefaultFineRate", "A taxa de multa não pode ser negativa.");
    }
}
