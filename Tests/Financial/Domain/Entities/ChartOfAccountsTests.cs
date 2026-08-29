using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class ChartOfAccountsTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateInstanceAndBeValid()
    {
        // Act
        var account = new ChartOfAccounts("1.1.01.001", "Banco Itaú C/C", AccountType.Asset, AccountNature.Debit);

        // Assert
        Assert.Equal("1.1.01.001", account.Code);
        Assert.Equal("Banco Itaú C/C", account.Name);
        Assert.Equal(AccountType.Asset, account.AccountType);
        Assert.Equal(AccountNature.Debit, account.Nature);
        Assert.False(account.IsSynthetic);
        Assert.True(account.AllowPosting);
        Assert.True(account.IsActive);
        Assert.False(account.IsInvalid);
        Assert.Empty(account.Notifications);
    }

    [Fact]
    public void Constructor_SyntheticAccount_ShouldForceAllowPostingFalse()
    {
        // Act
        var account = new ChartOfAccounts("1.1", "Ativo Circulante", AccountType.Asset, AccountNature.Debit, isSynthetic: true, allowPosting: true);

        // Assert
        Assert.True(account.IsSynthetic);
        Assert.False(account.AllowPosting);
        Assert.False(account.IsInvalid);
    }

    [Theory]
    [InlineData("", "Nome Válido", "Codigo", "O código do plano de contas é obrigatório.")]
    [InlineData("   ", "Nome Válido", "Codigo", "O código do plano de contas é obrigatório.")]
    [InlineData("1.1", "", "Nome", "O nome da conta é obrigatório.")]
    [InlineData("1.1", "   ", "Nome", "O nome da conta é obrigatório.")]
    public void Constructor_InvalidData_ShouldAddNotifications(string code, string name, string expectedKey, string expectedError)
    {
        // Act
        var account = new ChartOfAccounts(code, name, AccountType.Asset, AccountNature.Debit);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == expectedKey && n.Message == expectedError);
    }

    [Fact]
    public void Constructor_CodeExceedsMaxLength_ShouldAddNotification()
    {
        // Arrange
        var longCode = new string('1', 51);

        // Act
        var account = new ChartOfAccounts(longCode, "Conta Teste", AccountType.Asset, AccountNature.Debit);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Codigo" && n.Message.Contains("50 caracteres"));
    }

    [Fact]
    public void Constructor_NameExceedsMaxLength_ShouldAddNotification()
    {
        // Arrange
        var longName = new string('A', 151);

        // Act
        var account = new ChartOfAccounts("1.1.01", longName, AccountType.Asset, AccountNature.Debit);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Nome" && n.Message.Contains("150 caracteres"));
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdatePropertiesAndClearOldNotifications()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01.001", "Banco Itaú C/C", AccountType.Asset, AccountNature.Debit);

        // Act
        account.Update("1.1.01.002", "Banco Bradesco C/C", AccountType.Asset, AccountNature.Debit, null, false, true);

        // Assert
        Assert.Equal("1.1.01.002", account.Code);
        Assert.Equal("Banco Bradesco C/C", account.Name);
        Assert.NotNull(account.UpdatedAt);
        Assert.False(account.IsInvalid);
    }

    [Fact]
    public void Update_WithInvalidData_ShouldAddNotifications()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01.001", "Banco Itaú C/C", AccountType.Asset, AccountNature.Debit);

        // Act
        account.Update("", "", AccountType.Asset, AccountNature.Debit, null, false, true);

        // Assert
        Assert.True(account.IsInvalid);
        Assert.Contains(account.Notifications, n => n.Key == "Codigo");
        Assert.Contains(account.Notifications, n => n.Key == "Nome");
    }

    [Fact]
    public void DeactivateAndActivate_ShouldToggleStatusAndSetUpdatedAt()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01.001", "Banco Itaú C/C", AccountType.Asset, AccountNature.Debit);

        // Act
        account.Deactivate();

        // Assert
        Assert.False(account.IsActive);
        Assert.NotNull(account.UpdatedAt);

        // Act
        account.Activate();

        // Assert
        Assert.True(account.IsActive);
    }
}
