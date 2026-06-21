using Domain.Inventory.Entities;
using Xunit;

namespace Tests.Domain.Inventory.Entities;

public class InventoryTransactionItemTests
{
    [Fact]
    public void Should_Be_Valid_When_All_Properties_Are_Correct()
    {
        var item = new InventoryTransactionItem(
            transactionId: 1, 
            productId: 100, 
            uomId: 5, 
            transactionQuantity: 10m, 
            baseQuantity: 100m, 
            sourceWarehouseId: 1, 
            destinationWarehouseId: 2);
        
        Assert.False(item.IsInvalid);
        Assert.Equal(100, item.ProductId);
    }

    [Fact]
    public void Should_Be_Invalid_When_Quantities_Are_Zero()
    {
        var item = new InventoryTransactionItem(1, 100, 5, 0m, 0m);
        
        Assert.True(item.IsInvalid);
        Assert.Contains(item.Notifications, x => x.Message == "Quantidade deve ser maior que zero.");
        Assert.Contains(item.Notifications, x => x.Message == "Quantidade base deve ser maior que zero.");
    }
    
    [Fact]
    public void Should_Be_Invalid_When_Product_Or_Uom_Is_Zero()
    {
        var item = new InventoryTransactionItem(1, 0, 0, 10m, 100m);
        
        Assert.True(item.IsInvalid);
        Assert.Contains(item.Notifications, x => x.Message == "Produto é obrigatório.");
    }
}
