using Domain.Financial.Enums;
using Domain.Identity.Entities;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class FinancialDocument : BaseEntity
{
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = null!;
    public int PersonId { get; private set; }
    public int ChartOfAccountsId { get; private set; }
    public int? CostCenterId { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal FineAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int? ApprovalUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Notes { get; private set; }

    public Person Person { get; private set; } = null!;
    public ChartOfAccounts ChartOfAccounts { get; private set; } = null!;
    public CostCenter? CostCenter { get; private set; }
    public User? ApprovalUser { get; private set; }

    private readonly List<FinancialInstallment> _installments = new();
    public IReadOnlyCollection<FinancialInstallment> Installments => _installments.AsReadOnly();

    private readonly List<FinancialCostCenterAllocation> _allocations = new();
    public IReadOnlyCollection<FinancialCostCenterAllocation> Allocations => _allocations.AsReadOnly();

    protected FinancialDocument() { }

    public FinancialDocument(
        DocumentType documentType,
        string documentNumber,
        int personId,
        int chartOfAccountsId,
        DateTime issueDate,
        DateTime dueDate,
        decimal totalAmount,
        int? costCenterId = null,
        decimal discountAmount = 0,
        decimal interestAmount = 0,
        decimal fineAmount = 0,
        string? notes = null,
        bool requiresApproval = false)
    {
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        PersonId = personId;
        ChartOfAccountsId = chartOfAccountsId;
        CostCenterId = costCenterId;
        IssueDate = issueDate;
        DueDate = dueDate;
        TotalAmount = totalAmount;
        DiscountAmount = discountAmount;
        InterestAmount = interestAmount;
        FineAmount = fineAmount;
        Notes = notes;
        PaidAmount = 0;
        NetAmount = (totalAmount + interestAmount + fineAmount) - discountAmount;
        Status = requiresApproval ? DocumentStatus.PendingApproval : DocumentStatus.Open;

        Validate();
    }

    public void AddInstallment(FinancialInstallment installment)
    {
        if (installment == null) return;
        _installments.Add(installment);
        RecalculateTotals();
    }

    public void AddAllocation(FinancialCostCenterAllocation allocation)
    {
        if (allocation == null) return;
        _allocations.Add(allocation);
    }

    public void ValidateAllocationsSum()
    {
        if (_allocations.Any())
        {
            decimal sum = _allocations.Sum(a => a.Percentage);
            if (Math.Abs(sum - 100.00m) > 0.01m)
            {
                AddNotification("RateioCentroDeCusto", "A soma dos rateios por Centro de Custo deve ser exatamente 100.00%.");
            }
        }
    }

    public void Approve(int approvalUserId)
    {
        if (Status != DocumentStatus.PendingApproval)
        {
            AddNotification("Status", "Apenas títulos pendentes de aprovação podem ser aprovados.");
            return;
        }

        ApprovalUserId = approvalUserId;
        ApprovedAt = DateTime.UtcNow;
        Status = DocumentStatus.Open;
        RegisterUpdate();
    }

    public void Cancel()
    {
        if (Status == DocumentStatus.Paid || Status == DocumentStatus.Partial)
        {
            AddNotification("Status", "Não é possível cancelar um título que já possui liquidações efetuadas.");
            return;
        }

        Status = DocumentStatus.Canceled;
        foreach (var installment in _installments)
        {
            installment.Cancel();
        }

        RegisterUpdate();
    }

    public void RecalculateTotals()
    {
        if (_installments.Any())
        {
            PaidAmount = _installments.Sum(i => i.PaidAmount);
            if (PaidAmount >= NetAmount && NetAmount > 0)
            {
                Status = DocumentStatus.Paid;
            }
            else if (PaidAmount > 0)
            {
                Status = DocumentStatus.Partial;
            }
        }
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(DocumentNumber))
            AddNotification("NumeroDocumento", "O número do documento é obrigatório.");
        else if (DocumentNumber.Length > 50)
            AddNotification("NumeroDocumento", "O número do documento não pode exceder 50 caracteres.");

        if (PersonId <= 0)
            AddNotification("Pessoa", "A pessoa associada (Cliente/Fornecedor) é obrigatória.");

        if (ChartOfAccountsId <= 0)
            AddNotification("PlanoDeContas", "A conta do plano de contas é obrigatória.");

        if (TotalAmount <= 0)
            AddNotification("ValorTotal", "O valor total do título deve ser maior que zero.");

        if (!string.IsNullOrEmpty(Notes) && Notes.Length > 500)
            AddNotification("Observacoes", "As observações não podem exceder 500 caracteres.");
    }
}
