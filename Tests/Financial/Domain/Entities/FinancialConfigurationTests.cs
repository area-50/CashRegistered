using Domain.Financial.Entities;
using FluentAssertions;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class FinancialConfigurationTests
{
    [Fact]
    public void Constructor_DefaultValues_ShouldBeUnlockedAndValid()
    {
        // Act
        var config = new FinancialConfiguration();

        // Assert
        config.ApprovalThresholdAmount.Should().Be(0m);
        config.EnableApprovalWorkflow.Should().BeFalse();
        config.AllowAutoApprovalForManagers.Should().BeFalse();
        config.DefaultInterestDailyRate.Should().Be(0m);
        config.DefaultFineRate.Should().Be(0m);
        config.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public void UpdateSettings_ValidValues_ShouldUpdateAndBeValid()
    {
        // Arrange
        var config = new FinancialConfiguration();

        // Act
        config.UpdateSettings(
            approvalThresholdAmount: 5000.00m,
            enableApprovalWorkflow: true,
            allowAutoApprovalForManagers: true,
            defaultInterestDailyRate: 0.033m,
            defaultFineRate: 2.0m
        );

        // Assert
        config.ApprovalThresholdAmount.Should().Be(5000.00m);
        config.EnableApprovalWorkflow.Should().BeTrue();
        config.AllowAutoApprovalForManagers.Should().BeTrue();
        config.DefaultInterestDailyRate.Should().Be(0.033m);
        config.DefaultFineRate.Should().Be(2.0m);
        config.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public void UpdateSettings_NegativeThreshold_ShouldAddNotification()
    {
        // Arrange
        var config = new FinancialConfiguration();

        // Act
        config.UpdateSettings(
            approvalThresholdAmount: -100m,
            enableApprovalWorkflow: true,
            allowAutoApprovalForManagers: false,
            defaultInterestDailyRate: 0m,
            defaultFineRate: 0m
        );

        // Assert
        config.IsInvalid.Should().BeTrue();
        config.Notifications.Should().Contain(n => n.Message.Contains("limite de alçada não pode ser negativo"));
    }
}
