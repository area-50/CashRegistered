using Application.Financial.UseCases;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Domain.Shared.Abstractions;
using Domain.Shared.Notifications;
using FluentAssertions;
using Moq;
using Shared.Financial.Request;
using Xunit;

namespace Tests.Financial.Application.UseCases;

public class FinancialConfigurationUseCaseTests
{
    private readonly Mock<IFinancialConfigurationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly FinancialConfigurationUseCase _useCase;

    public FinancialConfigurationUseCaseTests()
    {
        _repositoryMock = new Mock<IFinancialConfigurationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new FinancialConfigurationUseCase(
            _repositoryMock.Object,
            _notificationContext,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GetFinancialConfiguration_NoActiveConfig_ShouldCreateDefaultAndReturn()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveConfigurationAsync()).ReturnsAsync((FinancialConfiguration?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialConfiguration>()))
            .Callback<FinancialConfiguration>(c => typeof(BaseEntity).GetProperty("Id")?.SetValue(c, 1));

        // Act
        var result = await _useCase.GetFinancialConfiguration();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.ApprovalThresholdAmount.Should().Be(0m);
        result.EnableApprovalWorkflow.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FinancialConfiguration>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateFinancialConfiguration_ExistingConfig_ShouldUpdateAndReturnId()
    {
        // Arrange
        var existingConfig = new FinancialConfiguration();
        typeof(BaseEntity).GetProperty("Id")?.SetValue(existingConfig, 1);
        _repositoryMock.Setup(r => r.GetActiveConfigurationAsync()).ReturnsAsync(existingConfig);

        var request = new UpdateFinancialConfigurationRequest
        {
            ApprovalThresholdAmount = 5000m,
            EnableApprovalWorkflow = true,
            AllowAutoApprovalForManagers = true,
            DefaultInterestDailyRate = 0.033m,
            DefaultFineRate = 2.0m
        };

        // Act
        var result = await _useCase.UpdateFinancialConfiguration(request);

        // Assert
        result.Id.Should().Be(1);
        existingConfig.ApprovalThresholdAmount.Should().Be(5000m);
        existingConfig.EnableApprovalWorkflow.Should().BeTrue();
        _repositoryMock.Verify(r => r.Update(existingConfig), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
