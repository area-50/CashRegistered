using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class FinancialDocumentTests
{
    [Fact]
    public void Constructor_ValidPayableDocument_ShouldCalculateNetAmountCorrectly()
    {
        // Act
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "NF-1001",
            personId: 5,
            chartOfAccountsId: 12,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(30),
            totalAmount: 1000.00m,
            discountAmount: 50.00m,
            interestAmount: 10.00m,
            fineAmount: 20.00m);

        // Assert
        Assert.Equal(DocumentType.Payable, doc.DocumentType);
        Assert.Equal("NF-1001", doc.DocumentNumber);
        Assert.Equal(1000.00m, doc.TotalAmount);
        Assert.Equal(980.00m, doc.NetAmount); // (1000 + 10 + 20) - 50 = 980
        Assert.Equal(DocumentStatus.Open, doc.Status);
        Assert.False(doc.IsInvalid);
        Assert.Empty(doc.Notifications);
    }

    [Fact]
    public void Constructor_RequiresApproval_ShouldSetStatusToPendingApproval()
    {
        // Act
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "NF-50000",
            personId: 5,
            chartOfAccountsId: 12,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(30),
            totalAmount: 50000.00m,
            requiresApproval: true);

        // Assert
        Assert.Equal(DocumentStatus.PendingApproval, doc.Status);
        Assert.False(doc.IsInvalid);
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "",
            personId: 0,
            chartOfAccountsId: 0,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow,
            totalAmount: 0);

        // Assert
        Assert.True(doc.IsInvalid);
        Assert.Contains(doc.Notifications, n => n.Key == "NumeroDocumento" && n.Message == "O número do documento é obrigatório.");
        Assert.Contains(doc.Notifications, n => n.Key == "Pessoa" && n.Message == "A pessoa associada (Cliente/Fornecedor) é obrigatória.");
        Assert.Contains(doc.Notifications, n => n.Key == "PlanoDeContas" && n.Message == "A conta do plano de contas é obrigatória.");
        Assert.Contains(doc.Notifications, n => n.Key == "ValorTotal" && n.Message == "O valor total do título deve ser maior que zero.");
    }

    [Fact]
    public void Constructor_NotesExceedMaxLength_ShouldAddNotification()
    {
        // Arrange
        var longNotes = new string('N', 501);

        // Act
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "NF-1",
            personId: 1,
            chartOfAccountsId: 1,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow,
            totalAmount: 100.00m,
            notes: longNotes);

        // Assert
        Assert.True(doc.IsInvalid);
        Assert.Contains(doc.Notifications, n => n.Key == "Observacoes" && n.Message.Contains("500 caracteres"));
    }

    [Fact]
    public void Approve_PendingApprovalDocument_ShouldSetStatusToOpenAndRecordUser()
    {
        // Arrange
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "NF-50000",
            personId: 5,
            chartOfAccountsId: 12,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(30),
            totalAmount: 50000.00m,
            requiresApproval: true);

        // Act
        doc.Approve(approvalUserId: 99);

        // Assert
        Assert.Equal(DocumentStatus.Open, doc.Status);
        Assert.Equal(99, doc.ApprovalUserId);
        Assert.NotNull(doc.ApprovedAt);
        Assert.False(doc.IsInvalid);
    }

    [Fact]
    public void Approve_NonPendingApprovalDocument_ShouldAddNotification()
    {
        // Arrange
        var doc = new FinancialDocument(
            DocumentType.Payable,
            "NF-100",
            personId: 5,
            chartOfAccountsId: 12,
            issueDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow,
            totalAmount: 100.00m,
            requiresApproval: false); // Status is Open

        // Act
        doc.Approve(approvalUserId: 99);

        // Assert
        Assert.True(doc.IsInvalid);
        Assert.Contains(doc.Notifications, n => n.Key == "Status" && n.Message.ToLower().Contains("apenas títulos pendentes de aprovação podem ser aprovados"));
    }

    [Fact]
    public void Cancel_OpenDocument_ShouldSetStatusCanceledAndCancelInstallments()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-1", personId: 1, chartOfAccountsId: 1, issueDate: DateTime.UtcNow, dueDate: DateTime.UtcNow, totalAmount: 100.00m);
        var installment = new FinancialInstallment(1, DateTime.UtcNow, 100.00m);
        doc.AddInstallment(installment);

        // Act
        doc.Cancel();

        // Assert
        Assert.Equal(DocumentStatus.Canceled, doc.Status);
        Assert.Equal(InstallmentStatus.Canceled, installment.Status);
        Assert.False(doc.IsInvalid);
    }

    [Fact]
    public void Cancel_PaidDocument_ShouldAddNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-1", personId: 1, chartOfAccountsId: 1, issueDate: DateTime.UtcNow, dueDate: DateTime.UtcNow, totalAmount: 100.00m);
        var installment = new FinancialInstallment(1, DateTime.UtcNow, 100.00m);
        doc.AddInstallment(installment);
        installment.RegisterPayment(100.00m, 0, 0, 0);
        doc.RecalculateTotals();

        // Act
        doc.Cancel();

        // Assert
        Assert.True(doc.IsInvalid);
        Assert.Contains(doc.Notifications, n => n.Key == "Status" && n.Message.Contains("Não é possível cancelar um título que já possui liquidações"));
    }

    [Fact]
    public void ValidateAllocationsSum_UnbalancedPercentage_ShouldAddNotification()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-1", personId: 1, chartOfAccountsId: 1, issueDate: DateTime.UtcNow, dueDate: DateTime.UtcNow, totalAmount: 100.00m);
        doc.AddAllocation(new FinancialCostCenterAllocation(costCenterId: 1, percentage: 40.00m, amount: 40.00m));
        doc.AddAllocation(new FinancialCostCenterAllocation(costCenterId: 2, percentage: 50.00m, amount: 50.00m));

        // Act
        doc.ValidateAllocationsSum();

        // Assert
        Assert.True(doc.IsInvalid);
        Assert.Contains(doc.Notifications, n => n.Key == "RateioCentroDeCusto" && n.Message.Contains("100.00%"));
    }

    [Fact]
    public void ValidateAllocationsSum_Exactly100Percent_ShouldBeValid()
    {
        // Arrange
        var doc = new FinancialDocument(DocumentType.Payable, "NF-1", personId: 1, chartOfAccountsId: 1, issueDate: DateTime.UtcNow, dueDate: DateTime.UtcNow, totalAmount: 100.00m);
        doc.AddAllocation(new FinancialCostCenterAllocation(costCenterId: 1, percentage: 40.00m, amount: 40.00m));
        doc.AddAllocation(new FinancialCostCenterAllocation(costCenterId: 2, percentage: 60.00m, amount: 60.00m));

        // Act
        doc.ValidateAllocationsSum();

        // Assert
        Assert.False(doc.IsInvalid);
    }
}
