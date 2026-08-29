using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class FinancialInstallmentTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateInstance()
    {
        // Act
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);

        // Assert
        Assert.Equal(1, installment.InstallmentNumber);
        Assert.Equal(1000.00m, installment.Amount);
        Assert.Equal(0, installment.PaidAmount);
        Assert.Equal(InstallmentStatus.Pending, installment.Status);
        Assert.False(installment.IsInvalid);
        Assert.Empty(installment.Notifications);
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var installment = new FinancialInstallment(installmentNumber: 0, dueDate: DateTime.UtcNow, amount: 0);

        // Assert
        Assert.True(installment.IsInvalid);
        Assert.Contains(installment.Notifications, n => n.Key == "NumeroParcela" && n.Message == "O número da parcela deve ser maior que zero.");
        Assert.Contains(installment.Notifications, n => n.Key == "ValorNominal" && n.Message == "O valor nominal da parcela deve ser maior que zero.");
    }

    [Fact]
    public void RegisterPayment_PartialAmount_ShouldUpdatePaidAmountAndSetStatusPartial()
    {
        // Arrange
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);

        // Act
        installment.RegisterPayment(amountPaid: 400.00m, discountApplied: 0, interestApplied: 0, fineApplied: 0);

        // Assert
        Assert.Equal(400.00m, installment.PaidAmount);
        Assert.Equal(InstallmentStatus.Partial, installment.Status);
        Assert.False(installment.IsInvalid);
    }

    [Fact]
    public void RegisterPayment_FullAmount_ShouldSetStatusPaid()
    {
        // Arrange
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);

        // Act
        installment.RegisterPayment(amountPaid: 1000.00m, discountApplied: 0, interestApplied: 0, fineApplied: 0);

        // Assert
        Assert.Equal(1000.00m, installment.PaidAmount);
        Assert.Equal(InstallmentStatus.Paid, installment.Status);
        Assert.False(installment.IsInvalid);
    }

    [Fact]
    public void RegisterPayment_ZeroOrNegativeAmountPaid_ShouldAddNotification()
    {
        // Arrange
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);

        // Act
        installment.RegisterPayment(amountPaid: 0, discountApplied: 0, interestApplied: 0, fineApplied: 0);

        // Assert
        Assert.True(installment.IsInvalid);
        Assert.Contains(installment.Notifications, n => n.Key == "ValorPago" && n.Message.Contains("maior que zero"));
    }

    [Fact]
    public void RegisterPayment_CanceledInstallment_ShouldAddNotification()
    {
        // Arrange
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);
        installment.Cancel();

        // Act
        installment.RegisterPayment(amountPaid: 1000.00m, discountApplied: 0, interestApplied: 0, fineApplied: 0);

        // Assert
        Assert.True(installment.IsInvalid);
        Assert.Contains(installment.Notifications, n => n.Key == "Status" && n.Message.Contains("cancelada"));
    }

    [Fact]
    public void Cancel_PendingInstallment_ShouldSetStatusCanceled()
    {
        // Arrange
        var installment = new FinancialInstallment(installmentNumber: 1, dueDate: DateTime.UtcNow.AddDays(10), amount: 1000.00m);

        // Act
        installment.Cancel();

        // Assert
        Assert.Equal(InstallmentStatus.Canceled, installment.Status);
        Assert.False(installment.IsInvalid);
    }
}
