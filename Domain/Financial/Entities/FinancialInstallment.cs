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

    public void AttachToDocument(int documentId)
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

        Status = PaidAmount >= netRequiredAmount ? InstallmentStatus.Paid : InstallmentStatus.Partial;

        RegisterUpdate();
    }

    public void Cancel()
    {
        Status = InstallmentStatus.Canceled;
        RegisterUpdate();
    }

    public InstallmentSettlementCalculation CalculateSettlement(
        DateTime paymentDate,
        decimal fineRate,
        decimal interestDailyRate,
        decimal? overrideDiscount = null,
        decimal? overrideInterest = null,
        decimal? overrideFine = null)
    {
        decimal remainingBalance = Math.Max(0, Amount - PaidAmount);

        // 🛡️ TRAVA 1: Se o pagamento for na data de vencimento ou anterior, NADA É FEITO.
        if (paymentDate.Date <= DueDate.Date)
        {
            decimal discount = overrideDiscount ?? 0m;
            decimal fine = overrideFine ?? 0m;
            decimal interest = overrideInterest ?? 0m;

            return new InstallmentSettlementCalculation
            {
                InstallmentId = Id,
                IsOverdue = false,
                OverdueDays = 0,
                RemainingBalance = remainingBalance,
                DailyInterestAmount = 0m,
                CalculatedFineAmount = 0m,
                CalculatedInterestAmount = 0m,
                FineToApply = fine,
                InterestToApply = interest,
                DiscountToApply = discount,
                SuggestedAmountPaid = Math.Max(0, remainingBalance + fine + interest - discount)
            };
        }

        // 🛡️ TRAVA 2: Se está em atraso, porém as taxas do documento estão zeradas, NADA É FEITO no cálculo de juros/multa.
        if (fineRate <= 0m && interestDailyRate <= 0m)
        {
            int daysOverdue = (paymentDate.Date - DueDate.Date).Days;
            decimal discount = overrideDiscount ?? 0m;
            decimal fine = overrideFine ?? 0m;
            decimal interest = overrideInterest ?? 0m;

            return new InstallmentSettlementCalculation
            {
                InstallmentId = Id,
                IsOverdue = true,
                OverdueDays = daysOverdue,
                RemainingBalance = remainingBalance,
                DailyInterestAmount = 0m,
                CalculatedFineAmount = 0m,
                CalculatedInterestAmount = 0m,
                FineToApply = fine,
                InterestToApply = interest,
                DiscountToApply = discount,
                SuggestedAmountPaid = Math.Max(0, remainingBalance + fine + interest - discount)
            };
        }

        // 🧮 SOMENTE SE PASSOU PELAS DUAS TRAVAS: Executa os cálculos.
        int overdueDays = (paymentDate.Date - DueDate.Date).Days;

        decimal calculatedFine = fineRate > 0m
            ? Math.Round(remainingBalance * (fineRate / 100m), 2)
            : 0m;

        decimal dailyInterest = interestDailyRate > 0m
            ? Math.Round(remainingBalance * (interestDailyRate / 100m), 2)
            : 0m;

        decimal calculatedInterest = Math.Round(dailyInterest * overdueDays, 2);

        decimal fineToApply = overrideFine ?? calculatedFine;
        decimal interestToApply = overrideInterest ?? calculatedInterest;
        decimal discountToApply = overrideDiscount ?? 0m;

        decimal suggestedAmountPaid = Math.Max(0, remainingBalance + fineToApply + interestToApply - discountToApply);

        return new InstallmentSettlementCalculation
        {
            InstallmentId = Id,
            IsOverdue = true,
            OverdueDays = overdueDays,
            RemainingBalance = remainingBalance,
            DailyInterestAmount = dailyInterest,
            CalculatedFineAmount = calculatedFine,
            CalculatedInterestAmount = calculatedInterest,
            FineToApply = fineToApply,
            InterestToApply = interestToApply,
            DiscountToApply = discountToApply,
            SuggestedAmountPaid = Math.Round(suggestedAmountPaid, 2)
        };
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

public struct InstallmentSettlementCalculation
{
    public int InstallmentId { get; set; }
    public bool IsOverdue { get; set; }
    public int OverdueDays { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal DailyInterestAmount { get; set; }
    public decimal CalculatedFineAmount { get; set; }
    public decimal CalculatedInterestAmount { get; set; }
    public decimal FineToApply { get; set; }
    public decimal InterestToApply { get; set; }
    public decimal DiscountToApply { get; set; }
    public decimal SuggestedAmountPaid { get; set; }
}

