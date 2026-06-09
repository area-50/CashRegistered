using Domain.Inventory.Entities;
using FluentAssertions;
using Xunit;

namespace Tests.Domain.Inventory.Entities;

public class StockBalanceTests
{
    [Fact]
    public void TotalQuantity_ShouldBeSumOfAvailableAndReserved()
    {
        var balance = new StockBalance(1, 1, 10, 5);

        balance.TotalQuantity.Should().Be(15);
    }
}
