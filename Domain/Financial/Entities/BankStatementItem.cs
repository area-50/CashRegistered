using Domain.Financial.Enums;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class BankStatementItem : BaseEntity
{
    public int BankStatementImportId { get; private set; }
    
    public DateTime TransactionDate { get; private set; }
    
    public string Description { get; private set; } = null!;
    
    public decimal Amount { get; private set; }
    
    public string FitId { get; private set; } = null!;
    
    public string? CheckNumber { get; private set; }
    
    public JournalEntryType TransactionType { get; private set; }
    
    public int? PaymentTransactionId { get; private set; }
    
    public BankStatementReconciliationStatus ReconciliationStatus { get; private set; }
    
    public DateTime? ReconciledAt { get; private set; }

    public BankStatementImport BankStatementImport { get; private set; } = null!;
    
    public PaymentTransaction? PaymentTransaction { get; private set; }

    protected BankStatementItem() { }

    public BankStatementItem(
        DateTime transactionDate,
        string description,
        decimal amount,
        string fitId,
        JournalEntryType transactionType,
        string? checkNumber = null)
    {
        TransactionDate = transactionDate;
        Description = description;
        Amount = amount;
        FitId = fitId;
        TransactionType = transactionType;
        CheckNumber = checkNumber;
        ReconciliationStatus = BankStatementReconciliationStatus.Pending;

        Validate();
    }

    internal void AttachToImport(int importId)
    {
        BankStatementImportId = importId;
    }

    public void MatchWithTransaction(int paymentTransactionId, bool isAutomatic = true)
    {
        PaymentTransactionId = paymentTransactionId;
        ReconciliationStatus = isAutomatic ? BankStatementReconciliationStatus.Matched : BankStatementReconciliationStatus.ManualMatched;
        ReconciledAt = DateTime.UtcNow;
        RegisterUpdate();
    }

    public void Ignore()
    {
        ReconciliationStatus = BankStatementReconciliationStatus.Ignored;
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(Description))
            AddNotification("Descricao", "A descrição da transação no extrato é obrigatória.");
        else if (Description.Length > 255)
            AddNotification("Descricao", "A descrição não pode exceder 255 caracteres.");

        if (string.IsNullOrWhiteSpace(FitId))
            AddNotification("FitIdExtrato", "O identificador único OFX (FitId) é obrigatório.");
        else if (FitId.Length > 100)
            AddNotification("FitIdExtrato", "O FitId não pode exceder 100 caracteres.");

        if (Amount <= 0)
            AddNotification("ValorTransacao", "O valor da transação no extrato deve ser maior que zero.");

        if (!string.IsNullOrEmpty(CheckNumber) && CheckNumber.Length > 50)
            AddNotification("NumeroCheque", "O número do cheque não pode exceder 50 caracteres.");
    }
}
