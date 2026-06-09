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

public class UnitOfMeasureUseCaseTests
{
    private readonly Mock<IUnitOfMeasureRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly UnitOfMeasureUseCase _useCase;

    public UnitOfMeasureUseCaseTests()
    {
        _repositoryMock = new Mock<IUnitOfMeasureRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new UnitOfMeasureUseCase(_repositoryMock.Object, _unitOfWorkMock.Object, _notificationContext);
    }

    [Fact]
    public async Task CreateUnitOfMeasure_ShouldCreateSuccessfully()
    {
        var request = new CreateUnitOfMeasureRequest { Code = "KG", Name = "Kilogram", AllowDecimals = true };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<UnitOfMeasure>()))
            .Callback<UnitOfMeasure>(u => typeof(BaseEntity).GetProperty("Id")?.SetValue(u, 1));

        var result = await _useCase.CreateUnitOfMeasure(request);

        result.Id.Should().NotBe(0);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<UnitOfMeasure>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
