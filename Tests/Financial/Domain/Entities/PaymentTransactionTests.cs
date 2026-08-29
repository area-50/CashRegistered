using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class PaymentTransactionTests
{
    [Fact]
    public void Constructor_ValidPaymentTransaction_ShouldCreateInstance()
    {
        // Act
        var transaction = new PaymentTransaction(
            financialInstallmentId: 10,
            financialAccountId: 2,
            paymentDate: DateTime.UtcNow,
            amountPaid: 500.00m,
            paymentMethod: PaymentMethod.Pix,
            createdByUserId: 1,
            discountApplied: 10.00m,
            interestApplied: 5.00m,
            fineApplied: 0,
            transactionReceiptNumber: "PIX-99887766");

        // Assert
        Assert.Equal(10, transaction.FinancialInstallmentId);
        Assert.Equal(2, transaction.FinancialAccountId);
        Assert.Equal(500.00m, transaction.AmountPaid);
        Assert.Equal(PaymentMethod.Pix, transaction.PaymentMethod);
        Assert.Equal("PIX-99887766", transaction.TransactionReceiptNumber);
        Assert.False(transaction.IsInvalid);
        Assert.Empty(transaction.Notifications);
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var transaction = new PaymentTransaction(
            financialInstallmentId: 0,
            financialAccountId: 0,
            paymentDate: DateTime.UtcNow,
            amountPaid: 0,
            paymentMethod: PaymentMethod.Cash,
            createdByUserId: 0);

        // Assert
        Assert.True(transaction.IsInvalid);
        Assert.Contains(transaction.Notifications, n => n.Key == "Parcela" && n.Message == "A parcela do título é obrigatória.");
        Assert.Contains(transaction.Notifications, n => n.Key == "ContaBancaria" && n.Message == "A conta financeira é obrigatória.");
        Assert.Contains(transaction.Notifications, n => n.Key == "UsuarioCriador" && n.Message == "O usuário criador da baixa é obrigatório.");
        Assert.Contains(transaction.Notifications, n => n.Key == "ValorPago" && n.Message == "O valor pago na liquidação deve ser maior que zero.");
    }

    [Fact]
    public void Constructor_ExceedingLengths_ShouldAddNotifications()
    {
        // Arrange
        var longReceipt = new string('R', 101);
        var longNotes = new string('N', 501);

        // Act
        var transaction = new PaymentTransaction(
            financialInstallmentId: 1,
            financialAccountId: 1,
            paymentDate: DateTime.UtcNow,
            amountPaid: 10.00m,
            paymentMethod: PaymentMethod.Cash,
            createdByUserId: 1,
            transactionReceiptNumber: longReceipt,
            notes: longNotes);

        // Assert
        Assert.True(transaction.IsInvalid);
        Assert.Contains(transaction.Notifications, n => n.Key == "Comprovante");
        Assert.Contains(transaction.Notifications, n => n.Key == "Observacoes");
    }

    [Fact]
    public void LinkJournalEntry_ValidId_ShouldSetJournalEntryId()
    {
        // Arrange
        var transaction = new PaymentTransaction(
            financialInstallmentId: 1,
            financialAccountId: 1,
            paymentDate: DateTime.UtcNow,
            amountPaid: 100.00m,
            paymentMethod: PaymentMethod.Pix,
            createdByUserId: 1);

        // Act
        transaction.LinkJournalEntry(journalEntryId: 555);

        // Assert
        Assert.Equal(555, transaction.JournalEntryId);
        Assert.NotNull(transaction.UpdatedAt);
    }
}
