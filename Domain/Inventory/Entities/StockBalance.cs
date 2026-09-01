using Domain.Shared.Abstractions;
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

    public void AddStock(decimal quantity)
    {
        if (quantity <= 0)
        {
            AddNotification("StockBalance", "A quantidade de adição deve ser maior que zero.");
            return;
        }
        AvailableQuantity += quantity;
    }

    public void RemoveStock(decimal quantity)
    {
        if (quantity <= 0)
        {
            AddNotification("StockBalance", "A quantidade de baixa deve ser maior que zero.");
            return;
        }
        if (AvailableQuantity < quantity)
        {
            AddNotification("StockBalance", "Saldo insuficiente neste almoxarifado para concluir a saída.");
            return;
        }
        AvailableQuantity -= quantity;
    }

    public void Reserve(decimal quantity)
    {
        if (quantity <= 0)
        {
            AddNotification("StockBalance", "A quantidade de reserva deve ser maior que zero.");
            return;
        }
        if (AvailableQuantity < quantity)
        {
            AddNotification("StockBalance", "Saldo disponível insuficiente para realizar a reserva.");
            return;
        }
        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
    }

    public void ReleaseReservation(decimal quantity)
    {
        if (quantity <= 0) return;
        if (ReservedQuantity < quantity)
        {
            AddNotification("StockBalance", "Quantidade a ser liberada é maior que o saldo reservado.");
            return;
        }
        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
    }

    public void ConsumeReservation(decimal quantity)
    {
        if (quantity <= 0) return;
        if (ReservedQuantity < quantity)
        {
            AddNotification("StockBalance", "Quantidade a ser consumida é maior que o saldo reservado.");
            return;
        }
        ReservedQuantity -= quantity;
        // Do not add to AvailableQuantity because it was actually consumed (exited physical stock)
    }
}
