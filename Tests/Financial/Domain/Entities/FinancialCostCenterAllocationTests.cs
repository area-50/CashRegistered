using Domain.Financial.Entities;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class FinancialCostCenterAllocationTests
{
    [Fact]
    public void Constructor_ValidAllocation_ShouldCreateInstance()
    {
        // Act
        var allocation = new FinancialCostCenterAllocation(costCenterId: 1, percentage: 50.00m, amount: 200.00m);

        // Assert
        Assert.Equal(1, allocation.CostCenterId);
        Assert.Equal(50.00m, allocation.Percentage);
        Assert.Equal(200.00m, allocation.Amount);
        Assert.False(allocation.IsInvalid);
        Assert.Empty(allocation.Notifications);
    }

    [Theory]
    [InlineData(0, 50.00, 100.00, "CentroDeCusto", "O Centro de Custo é obrigatório.")]
    [InlineData(1, 0, 100.00, "PorcentagemRateio", "A porcentagem de rateio deve estar entre 0.01% e 100.00%.")]
    [InlineData(1, 100.01, 100.00, "PorcentagemRateio", "A porcentagem de rateio deve estar entre 0.01% e 100.00%.")]
    [InlineData(1, 50.00, -1.00, "ValorRateio", "O valor rateado não pode ser negativo.")]
    public void Constructor_InvalidData_ShouldAddNotifications(int costCenterId, decimal percentage, decimal amount, string expectedKey, string expectedMessage)
    {
        // Act
        var allocation = new FinancialCostCenterAllocation(costCenterId, percentage, amount);

        // Assert
        Assert.True(allocation.IsInvalid);
        Assert.Contains(allocation.Notifications, n => n.Key == expectedKey && n.Message == expectedMessage);
    }
}
