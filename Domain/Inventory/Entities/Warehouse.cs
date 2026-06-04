using Flunt.Br;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class Warehouse : BaseEntity
{
    public Warehouse(string name, string type)
    {
        Name = name;
        Type = type;
        
        EntityValidate();
    }

    protected Warehouse() { }

    public string Name { get; private set; }
    
    public string Type { get; private set; }
    
    public ICollection<StockBalance> StockBalances { get; private set; } = new List<StockBalance>();

    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsNotNullOrEmpty(Name, "Nome", "O nome do almoxarifado é obrigatório.")
            .IsNotNullOrEmpty(Type, "Tipo", "O tipo do almoxarifado é obrigatório.");
        
        AddNotifications(contract.Notifications);
    }

    public static bool NotExists(Warehouse? warehouse, NotificationContext notificationContext)
    {
        if (warehouse != null) return false;
        notificationContext.AddNotification("Almoxarifado", "O almoxarifado não existe.");
        return true;
    }

    public void Update(string name, string type, bool isActive)
    {
        Name = name;
        Type = type;
        
        UpdateActivation(isActive);
        EntityValidate();
        RegisterUpdate();
    }
}
