using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Xunit;

namespace Tests.Application.Inventory.UseCases;

public class UomConversionUseCaseTests
{
    private readonly Mock<IUomConversionRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly UomConversionUseCase _useCase;

    public UomConversionUseCaseTests()
    {
        _repositoryMock = new Mock<IUomConversionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new UomConversionUseCase(_repositoryMock.Object, _unitOfWorkMock.Object, _notificationContext);
    }

    [Fact]
    public async Task CreateUomConversion_ShouldCreateSuccessfully()
    {
        var request = new CreateUomConversionRequest 
        { 
            FromUomId = 1, 
            ToUomId = 2, 
            Multiplier = 1.5m, 
            ProductId = 1 
        };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<UomConversion>()))
            .Callback<UomConversion>(u => typeof(BaseEntity).GetProperty("Id")?.SetValue(u, 1));

        var result = await _useCase.CreateUomConversion(request);

        result.Id.Should().NotBe(0);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<UomConversion>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
