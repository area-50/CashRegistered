using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class BankStatementItemTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateInstance()
    {
        // Act
        var item = new BankStatementItem(DateTime.UtcNow, "Tarifa de Extrato", 15.00m, "FIT-12345", JournalEntryType.Debit);

        // Assert
        Assert.Equal("Tarifa de Extrato", item.Description);
        Assert.Equal(15.00m, item.Amount);
        Assert.Equal("FIT-12345", item.FitId);
        Assert.Equal(JournalEntryType.Debit, item.TransactionType);
        Assert.Equal(BankStatementReconciliationStatus.Pending, item.ReconciliationStatus);
        Assert.False(item.IsInvalid);
        Assert.Empty(item.Notifications);
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var item = new BankStatementItem(DateTime.UtcNow, "", 0, "", JournalEntryType.Debit);

        // Assert
        Assert.True(item.IsInvalid);
        Assert.Contains(item.Notifications, n => n.Key == "Descricao" && n.Message == "A descrição da transação no extrato é obrigatória.");
        Assert.Contains(item.Notifications, n => n.Key == "FitIdExtrato" && n.Message == "O identificador único OFX (FitId) é obrigatório.");
        Assert.Contains(item.Notifications, n => n.Key == "ValorTransacao" && n.Message == "O valor da transação no extrato deve ser maior que zero.");
    }

    [Fact]
    public void MatchWithTransaction_Automatic_ShouldSetStatusMatched()
    {
        // Arrange
        var item = new BankStatementItem(DateTime.UtcNow, "Pix Recebido", 100.00m, "FIT-99", JournalEntryType.Credit);

        // Act
        item.MatchWithTransaction(paymentTransactionId: 77, isAutomatic: true);

        // Assert
        Assert.Equal(77, item.PaymentTransactionId);
        Assert.Equal(BankStatementReconciliationStatus.Matched, item.ReconciliationStatus);
        Assert.NotNull(item.ReconciledAt);
    }

    [Fact]
    public void MatchWithTransaction_Manual_ShouldSetStatusManualMatched()
    {
        // Arrange
        var item = new BankStatementItem(DateTime.UtcNow, "Pix Recebido", 100.00m, "FIT-99", JournalEntryType.Credit);

        // Act
        item.MatchWithTransaction(paymentTransactionId: 77, isAutomatic: false);

        // Assert
        Assert.Equal(BankStatementReconciliationStatus.ManualMatched, item.ReconciliationStatus);
    }

    [Fact]
    public void Ignore_ShouldSetStatusIgnored()
    {
        // Arrange
        var item = new BankStatementItem(DateTime.UtcNow, "Tarifa", 5.00m, "FIT-00", JournalEntryType.Debit);

        // Act
        item.Ignore();

        // Assert
        Assert.Equal(BankStatementReconciliationStatus.Ignored, item.ReconciliationStatus);
    }
}
