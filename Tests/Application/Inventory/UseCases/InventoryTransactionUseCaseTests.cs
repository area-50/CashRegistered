using System.Linq.Expressions;
using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Tests.Application.Inventory.UseCases;

public class InventoryTransactionUseCaseTests
{
    private readonly Mock<IInventoryTransactionRepository> _transactionRepoMock;
    private readonly Mock<IStockBalanceRepository> _stockBalanceRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly NotificationContext _notificationContext;
    private readonly InventoryTransactionUseCase _useCase;

    public InventoryTransactionUseCaseTests()
    {
        _transactionRepoMock = new Mock<IInventoryTransactionRepository>();
        _stockBalanceRepoMock = new Mock<IStockBalanceRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        
        _useCase = new InventoryTransactionUseCase(
            new List<global::Application.Inventory.Interfaces.IInventoryTransactionStrategy>(),
            _transactionRepoMock.Object,
            _uowMock.Object,
            _notificationContext
        );
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnZeroId_WhenNoItems()
    {
        var request = new CreateInventoryTransactionRequest
        {
            UserId = 1,
            TransactionType = "PurchaseEntry",
            Items = []
        };

        var response = await _useCase.CreateTransaction(request);

        response.Id.Should().Be(0);
        _notificationContext.Notifications.Should().Contain(x => x.Message.Contains("ao menos um item"));
    }

    [Fact]
    public async Task CreateTransaction_ShouldCallStrategy_WhenStrategyExists()
    {
        var request = new CreateInventoryTransactionRequest
        {
            UserId = 1,
            TransactionType = "PurchaseEntry",
            Items = new List<CreateInventoryTransactionItemRequest>
            {
                new() { ProductId = 1, DestinationWarehouseId = 2, BaseQuantity = 10, TransactionQuantity = 10 }
            }
        };

        var strategyMock = new Mock<global::Application.Inventory.Interfaces.IInventoryTransactionStrategy>();
        strategyMock.Setup(s => s.AppliesTo(global::Domain.Inventory.Enums.TransactionType.PurchaseEntry)).Returns(true);
        strategyMock.Setup(s => s.ProcessTransactionAsync(It.IsAny<InventoryTransaction>(), It.IsAny<IEnumerable<CreateInventoryTransactionItemRequest>>()))
            .Returns(Task.CompletedTask);

        var useCase = new InventoryTransactionUseCase(
            new List<global::Application.Inventory.Interfaces.IInventoryTransactionStrategy> { strategyMock.Object },
            _transactionRepoMock.Object,
            _uowMock.Object,
            _notificationContext
        );

        var response = await useCase.CreateTransaction(request);

        strategyMock.Verify(x => x.ProcessTransactionAsync(It.IsAny<InventoryTransaction>(), request.Items), Times.Once);
        _transactionRepoMock.Verify(x => x.CreateAsync(It.IsAny<InventoryTransaction>()), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
