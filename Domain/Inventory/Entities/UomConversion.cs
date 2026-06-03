using Flunt.Validations;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class UomConversion : BaseEntity
{
    public UomConversion(int fromUomId, int toUomId, decimal multiplier, int? productId = null)
    {
        FromUomId = fromUomId;
        ToUomId = toUomId;
        Multiplier = multiplier;
        ProductId = productId;

        EntityValidate();
    }

    protected UomConversion() { }

    public int FromUomId { get; set; }
    
    public UnitOfMeasure FromUom { get; set; }
    
    public int ToUomId { get; set; }
    
    public UnitOfMeasure ToUom { get; set; }
    
    public decimal Multiplier { get; set; }
    
    public int? ProductId { get; set; }
    
    public Product? Product { get; set; }

    public static bool UomConversionExists(UomConversion? uom, NotificationContext notificationContext)
    {
        if (uom != null) return true;
        notificationContext.AddNotification("Regra de conversão", "Regra de conversão não existe.");
        return false;
    }

    private void EntityValidate()
    {
        var contract = new Contract<UomConversion>()
            .Requires()
            .IsGreaterThan(FromUomId, 0, "Unidade Origem", "Unidade Origem é obrigatório.")
            .IsGreaterThan(ToUomId, 0, "Unidade Destino", "Unidade Destino é obrigatório.")
            .AreNotEquals(Multiplier, 0, "Multiplicador", "Deve ser diferente de zero.");
        AddNotifications(contract.Notifications);
    }

    public void UpdateUomConversion(
        int fromUomId,
        int toUomId,
        decimal multiplier,
        bool isActive,
        int? productId = null)
    {
        FromUomId = fromUomId;
        ToUomId = toUomId;
        Multiplier = multiplier;
        ProductId = productId;
        if (isActive)
            Activate();
        else Deactivate();
        
        EntityValidate();
        RegisterUpdate();
    }
}
