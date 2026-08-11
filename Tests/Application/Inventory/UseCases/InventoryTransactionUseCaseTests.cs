using System.Linq.Expressions;
using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Application.Inventory.Interfaces;

namespace Tests.Application.Inventory.UseCases;

public class InventoryTransactionUseCaseTests
{
    private readonly Mock<ITransactionStatusHandler> _statusChainMock;
    private readonly Mock<IInventoryTransactionRepository> _transactionRepoMock;
    private readonly Mock<IProductUseCase> _productUseCaseMock;
    private readonly Mock<IStockBalanceUseCase> _stockBalanceUseCaseMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly NotificationContext _notificationContext;
    private readonly InventoryTransactionUseCase _useCase;

    public InventoryTransactionUseCaseTests()
    {
        _statusChainMock = new Mock<ITransactionStatusHandler>();
        _transactionRepoMock = new Mock<IInventoryTransactionRepository>();
        _productUseCaseMock = new Mock<IProductUseCase>();
        _stockBalanceUseCaseMock = new Mock<IStockBalanceUseCase>();
        _uowMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _useCase = new InventoryTransactionUseCase(
            _statusChainMock.Object,
            _transactionRepoMock.Object,
            _productUseCaseMock.Object,
            _stockBalanceUseCaseMock.Object,
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
                new() { ProductId = 1, UomId = 1, DestinationWarehouseId = 2, BaseQuantity = 10, TransactionQuantity = 10 }
            }
        };

        var productMock = new Product("Test", "SKU1", 1, 1, null, null, null);
        _productUseCaseMock.Setup(p => p.GetById(1)).ReturnsAsync(productMock);

        var statusHandlerMock = new Mock<ITransactionStatusHandler>();
        statusHandlerMock.Setup(s => s.ProcessAsync(It.IsAny<InventoryTransaction>(), It.IsAny<IEnumerable<CreateInventoryTransactionItemRequest>>()))
            .Returns(Task.CompletedTask);

        var useCase = new InventoryTransactionUseCase(
            statusHandlerMock.Object,
            _transactionRepoMock.Object,
            _productUseCaseMock.Object,
            _stockBalanceUseCaseMock.Object,
            _uowMock.Object,
            _notificationContext
        );

        var response = await useCase.CreateTransaction(request);

        statusHandlerMock.Verify(x => x.ProcessAsync(It.IsAny<InventoryTransaction>(), It.IsAny<IEnumerable<CreateInventoryTransactionItemRequest>>()), Times.Once);
        _transactionRepoMock.Verify(x => x.CreateAsync(It.IsAny<InventoryTransaction>()), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
