using Shared.Abstractions;
using Flunt.Br;

namespace Domain.Inventory.Entities;

public class StockBalance : BaseEntity
{
    public StockBalance(
        int productId,
        int warehouseId,
        decimal availableQuantity = 0,
        decimal reservedQuantity = 0
    )
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = reservedQuantity;
        EntityValidate();
    }

    protected StockBalance() { }

    public int ProductId { get; private set; }
    
    public Product Product { get; private set; }
    
    public int WarehouseId { get; private set; }
    
    public Warehouse Warehouse { get; private set; }
    
    public decimal AvailableQuantity { get; private set; }
    
    public decimal ReservedQuantity { get; private set; }
    
    public decimal TotalQuantity => AvailableQuantity + ReservedQuantity;
    
    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsGreaterThan(
                ProductId, 0, "Não foi possível criar Saldo de estoque",
                "Produto não encontrado."
            )
            .IsGreaterThan(
                WarehouseId, 0, "Não foi possível criar Saldo de estoque",
                "Almoxarifado não encontrado."
            );
        AddNotifications(contract.Notifications);
    }
}
