using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class FinancialCostCenterAllocation : BaseEntity
{
    public int? FinancialDocumentId { get; private set; }
    public int? JournalEntryItemId { get; private set; }
    public int CostCenterId { get; private set; }
    public decimal Percentage { get; private set; }
    public decimal Amount { get; private set; }

    public FinancialDocument? FinancialDocument { get; private set; }
    public JournalEntryItem? JournalEntryItem { get; private set; }
    public CostCenter CostCenter { get; private set; } = null!;

    protected FinancialCostCenterAllocation() { }

    public FinancialCostCenterAllocation(
        int costCenterId,
        decimal percentage,
        decimal amount,
        int? financialDocumentId = null,
        int? journalEntryItemId = null)
    {
        CostCenterId = costCenterId;
        Percentage = percentage;
        Amount = amount;
        FinancialDocumentId = financialDocumentId;
        JournalEntryItemId = journalEntryItemId;

        Validate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (CostCenterId <= 0)
            AddNotification("CentroDeCusto", "O Centro de Custo é obrigatório.");

        if (Percentage <= 0 || Percentage > 100)
            AddNotification("PorcentagemRateio", "A porcentagem de rateio deve estar entre 0.01% e 100.00%.");

        if (Amount < 0)
            AddNotification("ValorRateio", "O valor rateado não pode ser negativo.");
    }
}
