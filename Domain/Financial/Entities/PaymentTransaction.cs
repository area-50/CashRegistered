using Domain.Financial.Enums;
using Domain.Identity.Entities;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class PaymentTransaction : BaseEntity
{
    public int FinancialInstallmentId { get; private set; }
    public int FinancialAccountId { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public decimal InterestApplied { get; private set; }
    public decimal FineApplied { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? TransactionReceiptNumber { get; private set; }
    public int? JournalEntryId { get; private set; }
    public int CreatedByUserId { get; private set; }
    public string? Notes { get; private set; }

    public FinancialInstallment FinancialInstallment { get; private set; } = null!;
    public FinancialAccount FinancialAccount { get; private set; } = null!;
    public JournalEntry? JournalEntry { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    protected PaymentTransaction() { }

    public PaymentTransaction(
        int financialInstallmentId,
        int financialAccountId,
        DateTime paymentDate,
        decimal amountPaid,
        PaymentMethod paymentMethod,
        int createdByUserId,
        decimal discountApplied = 0,
        decimal interestApplied = 0,
        decimal fineApplied = 0,
        string? transactionReceiptNumber = null,
        string? notes = null,
        int? journalEntryId = null)
    {
        FinancialInstallmentId = financialInstallmentId;
        FinancialAccountId = financialAccountId;
        PaymentDate = paymentDate;
        AmountPaid = amountPaid;
        PaymentMethod = paymentMethod;
        CreatedByUserId = createdByUserId;
        DiscountApplied = discountApplied;
        InterestApplied = interestApplied;
        FineApplied = fineApplied;
        TransactionReceiptNumber = transactionReceiptNumber;
        Notes = notes;
        JournalEntryId = journalEntryId;

        Validate();
    }

    public void LinkJournalEntry(int journalEntryId)
    {
        JournalEntryId = journalEntryId;
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (FinancialInstallmentId <= 0)
            AddNotification("Parcela", "A parcela do título é obrigatória.");

        if (FinancialAccountId <= 0)
            AddNotification("ContaBancaria", "A conta financeira é obrigatória.");

        if (CreatedByUserId <= 0)
            AddNotification("UsuarioCriador", "O usuário criador da baixa é obrigatório.");

        if (AmountPaid <= 0)
            AddNotification("ValorPago", "O valor pago na liquidação deve ser maior que zero.");

        if (!string.IsNullOrEmpty(TransactionReceiptNumber) && TransactionReceiptNumber.Length > 100)
            AddNotification("Comprovante", "O número do comprovante não pode exceder 100 caracteres.");

        if (!string.IsNullOrEmpty(Notes) && Notes.Length > 500)
            AddNotification("Observacoes", "As observações não podem exceder 500 caracteres.");
    }
}
