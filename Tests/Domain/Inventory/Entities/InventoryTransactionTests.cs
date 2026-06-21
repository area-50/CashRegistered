using Domain.Inventory.Entities;
using Domain.Inventory.Enums;
using Xunit;

namespace Tests.Domain.Inventory.Entities;

public class InventoryTransactionTests
{
    [Fact]
    public void Should_Be_Valid_When_All_Properties_Are_Correct()
    {
        var transaction = new InventoryTransaction(1, TransactionType.PurchaseEntry, "NF-1234");
        
        Assert.False(transaction.IsInvalid);
        Assert.Equal(1, transaction.UserId);
        Assert.Equal(TransactionType.PurchaseEntry, transaction.Type);
        Assert.Equal("NF-1234", transaction.ReferenceDocument);
    }

    [Fact]
    public void Should_Be_Invalid_When_UserId_Is_Zero()
    {
        var transaction = new InventoryTransaction(0, TransactionType.Transfer);
        
        Assert.True(transaction.IsInvalid);
        Assert.Contains(transaction.Notifications, x => x.Key == "Transação");
    }
}
