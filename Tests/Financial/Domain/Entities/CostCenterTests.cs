using Domain.Financial.Entities;
using FluentAssertions;
using Domain.Shared.Notifications;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class CostCenterTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateEntitySuccessfully()
    {
        // Arrange & Act
        var costCenter = new CostCenter("Tecnologia da Informação", 1);

        // Assert
        costCenter.Name.Should().Be("Tecnologia da Informação");
        costCenter.ManagerId.Should().Be(1);
        costCenter.IsActive.Should().BeTrue();
        costCenter.IsInvalid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidName_ShouldAddNotification(string? name)
    {
        // Arrange & Act
        var costCenter = new CostCenter(name!, 1);

        // Assert
        costCenter.IsInvalid.Should().BeTrue();
        costCenter.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("obrigatório"));
    }

    [Fact]
    public void Constructor_NameExceedingMaxLength_ShouldAddNotification()
    {
        // Arrange
        var longName = new string('A', 151);

        // Act
        var costCenter = new CostCenter(longName, 1);

        // Assert
        costCenter.IsInvalid.Should().BeTrue();
        costCenter.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("exceder 150 caracteres"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidManagerId_ShouldAddNotification(int managerId)
    {
        // Arrange & Act
        var costCenter = new CostCenter("Marketing", managerId);

        // Assert
        costCenter.IsInvalid.Should().BeTrue();
        costCenter.Notifications.Should().Contain(n => n.Key == "GerenteId" && n.Message.Contains("obrigatório"));
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var costCenter = new CostCenter("Vendas", 1);

        // Act
        costCenter.Update("Comercial", 2);

        // Assert
        costCenter.Name.Should().Be("Comercial");
        costCenter.ManagerId.Should().Be(2);
        costCenter.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public void NotExists_WhenCostCenterIsNull_ShouldReturnTrueAndAddNotification()
    {
        // Arrange
        CostCenter? costCenter = null;
        var notificationContext = new NotificationContext();

        // Act
        var result = CostCenter.NotExists(costCenter, notificationContext);

        // Assert
        result.Should().BeTrue();
        notificationContext.IsInvalid.Should().BeTrue();
        notificationContext.Notifications.Should().Contain(n => n.Key == "CentroDeCusto" && n.Message.Contains("não existe"));
    }

    [Fact]
    public void NotExists_WhenCostCenterIsNotNull_ShouldReturnFalse()
    {
        // Arrange
        var costCenter = new CostCenter("Recursos Humanos", 1);
        var notificationContext = new NotificationContext();

        // Act
        var result = CostCenter.NotExists(costCenter, notificationContext);

        // Assert
        result.Should().BeFalse();
        notificationContext.IsInvalid.Should().BeFalse();
    }
}
