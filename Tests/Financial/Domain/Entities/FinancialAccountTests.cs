using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class FinancialAccountTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateInstanceAndSetCurrentBalance()
    {
        // Act
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10, initialBalance: 500.00m);

        // Assert
        Assert.Equal("Caixa Tesouraria", account.Name);
        Assert.Equal(FinancialAccountType.Cash, account.AccountType);
        Assert.Equal(10, account.ChartOfAccountsId);
        Assert.Equal(500.00m, account.InitialBalance);
        Assert.Equal(500.00m, account.CurrentBalance);
        Assert.Equal("BRL", account.Currency);
        Assert.False(account.IsInvalid);
        Assert.Empty(account.Notifications);
    }

    [Fact]
    public void CreditBalance_ValidAmount_ShouldIncreaseCurrentBalance()
    {
        // Arrange
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10, initialBalance: 100.00m);

        // Act
        account.CreditBalance(250.50m);

        // Assert
        Assert.Equal(350.50m, account.CurrentBalance);
        Assert.False(account.IsInvalid);
    }

    [Fact]
    public void CreditBalance_NegativeAmount_ShouldAddNotification()
    {
        // Arrange
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10, initialBalance: 100.00m);

        // Act
        account.CreditBalance(-50.00m);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Valor" && n.Message.Contains("negativo"));
    }

    [Fact]
    public void DebitBalance_ValidAmount_ShouldDecreaseCurrentBalance()
    {
        // Arrange
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10, initialBalance: 500.00m);

        // Act
        account.DebitBalance(200.00m);

        // Assert
        Assert.Equal(300.00m, account.CurrentBalance);
        Assert.False(account.IsInvalid);
    }

    [Fact]
    public void DebitBalance_NegativeAmount_ShouldAddNotification()
    {
        // Arrange
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10, initialBalance: 500.00m);

        // Act
        account.DebitBalance(-100.00m);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Valor" && n.Message.Contains("negativo"));
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var account = new FinancialAccount("", FinancialAccountType.Cash, chartOfAccountsId: 0);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Nome" && n.Message == "O nome da conta financeira é obrigatório.");
        Assert.Contains(account.Notifications, n => n.Key == "PlanoDeContas" && n.Message == "A conta do plano de contas associada é obrigatória.");
    }

    [Fact]
    public void Constructor_ExceedingStringLengths_ShouldAddNotifications()
    {
        // Act
        var account = new FinancialAccount(
            name: new string('A', 101),
            accountType: FinancialAccountType.CheckingAccount,
            chartOfAccountsId: 1,
            bankCode: new string('1', 21),
            bankName: new string('B', 101),
            agencyNumber: new string('9', 21),
            accountNumber: new string('0', 31),
            currency: "USDT");

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Nome");
        Assert.Contains(account.Notifications, n => n.Key == "CodigoBanco");
        Assert.Contains(account.Notifications, n => n.Key == "NomeBanco");
        Assert.Contains(account.Notifications, n => n.Key == "Agencia");
        Assert.Contains(account.Notifications, n => n.Key == "ContaBancaria");
        Assert.Contains(account.Notifications, n => n.Key == "Moeda");
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateProperties()
    {
        // Arrange
        var account = new FinancialAccount("Caixa Tesouraria", FinancialAccountType.Cash, chartOfAccountsId: 10);

        // Act
        account.Update("Banco Itaú Principal", FinancialAccountType.CheckingAccount, chartOfAccountsId: 15, bankCode: "341", bankName: "Itaú");

        // Assert
        Assert.Equal("Banco Itaú Principal", account.Name);
        Assert.Equal(FinancialAccountType.CheckingAccount, account.AccountType);
        Assert.Equal(15, account.ChartOfAccountsId);
        Assert.Equal("341", account.BankCode);
        Assert.Equal("Itaú", account.BankName);
        Assert.False(account.IsInvalid);
    }
}
