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

public class WarehouseUseCaseTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly WarehouseUseCase _useCase;

    public WarehouseUseCaseTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new WarehouseUseCase(_repositoryMock.Object, _notificationContext, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateWarehouse_ShouldCreateSuccessfully()
    {
        var request = new CreateWarehouseRequest { Name = "W1", Type = "Physical" };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Warehouse>()))
            .Callback<Warehouse>(w => typeof(BaseEntity).GetProperty("Id")?.SetValue(w, 1));

        var result = await _useCase.CreateWarehouse(request);

        result.Id.Should().NotBe(0);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Warehouse>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateWarehouse_ShouldDeactivateSuccessfully()
    {
        var id = 1;
        var warehouse = new Warehouse("W1", "Physical");
        typeof(BaseEntity).GetProperty("Id")?.SetValue(warehouse, id);
        _repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(warehouse);

        await _useCase.DeactivateWarehouse(id);

        warehouse.IsActive.Should().BeFalse();
        _repositoryMock.Verify(x => x.Update(warehouse), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
