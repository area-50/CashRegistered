using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Application.Inventory.UseCases;

public class StockBalanceUseCaseTests
{
    private readonly Mock<IStockBalanceRepository> _repositoryMock;
    private readonly StockBalanceUseCase _useCase;

    public StockBalanceUseCaseTests()
    {
        _repositoryMock = new Mock<IStockBalanceRepository>();
        _useCase = new StockBalanceUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldCallRepository()
    {
        var balances = new List<StockBalance> { new(1, 1, 0, 0) };

        await _useCase.AddRangeAsync(balances);

        _repositoryMock.Verify(x => x.AddRangeAsync(balances), Times.Once);
    }
}
