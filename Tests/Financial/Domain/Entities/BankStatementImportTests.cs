using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class BankStatementImportTests
{
    [Fact]
    public void Constructor_ValidImport_ShouldCreateInstanceAndItems()
    {
        // Arrange
        var import = new BankStatementImport(
            financialAccountId: 2,
            fileName: "extrato_agosto.ofx",
            importDate: DateTime.UtcNow,
            startDate: DateTime.UtcNow.AddDays(-30),
            endDate: DateTime.UtcNow,
            createdByUserId: 1);

        var item = new BankStatementItem(
            transactionDate: DateTime.UtcNow,
            description: "Tarifa Bancária",
            amount: 15.00m,
            fitId: "FIT-123456789",
            transactionType: JournalEntryType.Debit
        );

        // Act=
        import.AddItem(item);

        // Assert
        Assert.Equal(2, import.FinancialAccountId);
        Assert.Equal("extrato_agosto.ofx", import.FileName);
        Assert.Single(import.Items);
        Assert.False(import.IsInvalid);
    }

    [Fact]
    public void MatchWithTransaction_ShouldSetReconciliationStatusMatched()
    {
        // Arrange
        var item = new BankStatementItem(
            transactionDate: DateTime.UtcNow,
            description: "Pagamento Recebido",
            amount: 100.00m,
            fitId: "FIT-99999",
            transactionType: JournalEntryType.Credit
        );

        // Act
        item.MatchWithTransaction(paymentTransactionId: 50, isAutomatic: true);

        // Assert
        Assert.Equal(50, item.PaymentTransactionId);
        Assert.Equal(BankStatementReconciliationStatus.Matched, item.ReconciliationStatus);
        Assert.NotNull(item.ReconciledAt);
    }

    [Fact]
    public void Constructor_FileNameExceeding255Chars_ShouldAddNotification()
    {
        // Arrange
        var longFileName = new string('F', 256) + ".ofx";

        // Act
        var import = new BankStatementImport(
            financialAccountId: 1,
            fileName: longFileName,
            importDate: DateTime.UtcNow,
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow,
            createdByUserId: 1);

        // Assert
        Assert.True(import.IsInvalid);
        Assert.Contains(import.Notifications, n => n.Key == "NomeArquivo" && n.Message.Contains("255 caracteres"));
    }
}
