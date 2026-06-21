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

    [Fact]
    public void AddStock_Should_Increase_Quantity()
    {
        var balance = new StockBalance(1, 1, 10, 0);
        
        balance.AddStock(5);
        
        balance.AvailableQuantity.Should().Be(15);
        balance.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public void RemoveStock_Should_Decrease_Quantity()
    {
        var balance = new StockBalance(1, 1, 10, 0);
        
        balance.RemoveStock(5);
        
        balance.AvailableQuantity.Should().Be(5);
        balance.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public void RemoveStock_Should_AddNotification_When_QuantityIsGreaterThanAvailable()
    {
        var balance = new StockBalance(1, 1, 10, 0);
        
        balance.RemoveStock(15);
        
        balance.IsInvalid.Should().BeTrue();
        balance.Notifications.Should().Contain(x => x.Key == "StockBalance");
        balance.AvailableQuantity.Should().Be(10); // não deve ter alterado
    }
}
