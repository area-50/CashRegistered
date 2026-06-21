using Shared.Abstractions;
using Flunt.Br;

namespace Domain.Inventory.Entities;

public class InventoryTransactionItem : BaseEntity
{
    protected InventoryTransactionItem() { } // EF Core

    public InventoryTransactionItem(
        int transactionId,
        int productId,
        int uomId,
        decimal transactionQuantity,
        decimal baseQuantity,
        int? sourceWarehouseId = null,
        int? destinationWarehouseId = null)
    {
        TransactionId = transactionId;
        ProductId = productId;
        UomId = uomId;
        TransactionQuantity = transactionQuantity;
        BaseQuantity = baseQuantity;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        
        EntityValidate();
    }

    public int TransactionId { get; private set; }
    public InventoryTransaction Transaction { get; private set; }
    
    public int ProductId { get; private set; }
    public Product Product { get; private set; }
    
    public int? SourceWarehouseId { get; private set; }
    public Warehouse? SourceWarehouse { get; private set; }
    
    public int? DestinationWarehouseId { get; private set; }
    public Warehouse? DestinationWarehouse { get; private set; }
    
    public int UomId { get; private set; }
    public UnitOfMeasure Uom { get; private set; }
    
    public decimal TransactionQuantity { get; private set; }
    public decimal BaseQuantity { get; private set; }

    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsGreaterThan(ProductId, 0, "Transação Item", "Produto é obrigatório.")
            .IsGreaterThan(UomId, 0, "Transação Item", "Unidade de Medida é obrigatória.")
            .IsGreaterThan(TransactionQuantity, 0, "Transação Item", "Quantidade deve ser maior que zero.")
            .IsGreaterThan(BaseQuantity, 0, "Transação Item", "Quantidade base deve ser maior que zero.");
            
        AddNotifications(contract.Notifications);
    }
}
