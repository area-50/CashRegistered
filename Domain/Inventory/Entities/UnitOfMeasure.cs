using Domain.Inventory.Repositories;

using Flunt.Notifications;
using Flunt.Validations;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class UnitOfMeasure : BaseEntity
{
    public UnitOfMeasure(string code, string name, bool allowDecimals = false)
    {
        Code = code;
        Name = name;
        AllowDecimals = allowDecimals;

        EntityValidate();
    }

    protected UnitOfMeasure() { }

    public string Code { get; set; }
    
    public string Name { get; set; }
    
    public bool AllowDecimals { get; set; }

    public void ChangeAllowDecimals(bool allowDecimals)
    {
        AllowDecimals = allowDecimals;
        RegisterUpdate();
    }

    public async Task<bool> CodeExists(IUnitOfMeasureRepository uomRepository, string requestCode)
    {
        var result = await uomRepository.FindAsync(uom => uom.Code == requestCode);
        if (!result.Any()) return false;
        AddNotification("Sigla", "Sigla já existe.");
        return true;
    }

    public static bool NotExists(UnitOfMeasure? uom,  NotificationContext notificationContext)
    {
        if (uom != null) return false;
        notificationContext.AddNotification("Unidade de Medida", "Unidade de Medida não existe");
        return true;
    }

    private void EntityValidate()
    {
        var contract = new Contract<Notification>()
            .Requires()
            .IsNotNullOrEmpty(
                Code,
                "Sigla/Código",
                "A sigla da unidade de medida é obrigatória."
            )
            .IsNotUrlOrEmpty(
                Name,
                "Nome da unidade de medida",
                "Nome da unidade de medida é obrigatório."
            );
        AddNotifications(contract.Notifications);
    }

    public void Update(string code, string name, bool isActive, bool allowDecimals = false)
    {
        Code = code;
        Name = name;
        AllowDecimals = allowDecimals;
        
        UpdateActivation(isActive);
        EntityValidate();
        RegisterUpdate();
    }
}
