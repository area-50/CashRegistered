using Domain.Financial.Enums;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class FinancialInstallment : BaseEntity
{
    public int FinancialDocumentId { get; private set; }
    
    public int InstallmentNumber { get; private set; }
    
    public DateTime DueDate { get; private set; }
    
    public decimal Amount { get; private set; }
    
    public decimal PaidAmount { get; private set; }
    
    public decimal DiscountAmount { get; private set; }
    
    public decimal InterestAmount { get; private set; }
    
    public decimal FineAmount { get; private set; }
    
    public InstallmentStatus Status { get; private set; }

    public FinancialDocument FinancialDocument { get; private set; } = null!;

    private readonly List<PaymentTransaction> _paymentTransactions = new();
    
    public IReadOnlyCollection<PaymentTransaction> PaymentTransactions => _paymentTransactions.AsReadOnly();

    protected FinancialInstallment() { }

    public FinancialInstallment(
        int installmentNumber,
        DateTime dueDate,
        decimal amount)
    {
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        Amount = amount;
        PaidAmount = 0;
        DiscountAmount = 0;
        InterestAmount = 0;
        FineAmount = 0;
        Status = InstallmentStatus.Pending;

        Validate();
    }

    internal void AttachToDocument(int documentId)
    {
        FinancialDocumentId = documentId;
    }

    public void RegisterPayment(decimal amountPaid, decimal discountApplied, decimal interestApplied, decimal fineApplied)
    {
        if (Status == InstallmentStatus.Canceled)
        {
            AddNotification("Status", "Não é possível liquidar uma parcela cancelada.");
            return;
        }

        if (amountPaid <= 0)
        {
            AddNotification("ValorPago", "O valor pago deve ser maior que zero.");
            return;
        }

        PaidAmount += amountPaid;
        DiscountAmount += discountApplied;
        InterestAmount += interestApplied;
        FineAmount += fineApplied;

        decimal netRequiredAmount = (Amount + InterestAmount + FineAmount) - DiscountAmount;

        if (PaidAmount >= netRequiredAmount)
        {
            Status = InstallmentStatus.Paid;
        }
        else
        {
            Status = InstallmentStatus.Partial;
        }

        RegisterUpdate();
    }

    public void Cancel()
    {
        Status = InstallmentStatus.Canceled;
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (InstallmentNumber <= 0)
            AddNotification("NumeroParcela", "O número da parcela deve ser maior que zero.");

        if (Amount <= 0)
            AddNotification("ValorNominal", "O valor nominal da parcela deve ser maior que zero.");
    }
}
