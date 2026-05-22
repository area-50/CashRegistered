using Flunt.Br;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class Tag : BaseEntity
{
    public Tag(string name, string? hexColor = null)
    {
        Name = name;
        HexColor = hexColor;
        
        EntityValidate();
    }

    protected Tag() { }

    public string Name { get; set; }
    
    public string? HexColor { get; set; }
    
    public ICollection<Product> Products { get; set; } = new List<Product>();

    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsNotNullOrEmpty(Name, "Nome", "O nome da tag é obrigatório.");
        AddNotifications(contract.Notifications);
    }
    
    public static bool NotExists(Tag? tag, NotificationContext notificationContext)
    {
        if (tag != null) return false;
        notificationContext.AddNotification("Tag", "A tag não existe");
        return true;

    }
}
