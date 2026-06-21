using System.Linq.Expressions;
using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Xunit;

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
            _transactionRepoMock.Object,
            _stockBalanceRepoMock.Object,
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
    public async Task CreateTransaction_ShouldUpdateStock_WhenPurchaseEntry()
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

        var balance = new StockBalance(1, 2, 5, 0);
        _stockBalanceRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<StockBalance, bool>>>()))
            .ReturnsAsync(new List<StockBalance> { balance });

        var response = await _useCase.CreateTransaction(request);

        _stockBalanceRepoMock.Verify(x => x.Update(It.Is<StockBalance>(b => b.AvailableQuantity == 15)), Times.Once);
        _transactionRepoMock.Verify(x => x.CreateAsync(It.IsAny<InventoryTransaction>()), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }
}
