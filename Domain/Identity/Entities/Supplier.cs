using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Identity.Entities;

public class Supplier : BaseEntity
{
    public Supplier(int personId)
    {
        PersonId = personId;
    }

    protected Supplier() { }

    public int PersonId { get; set; }
    
    public Person Person { get; set; }
    
    public ICollection<Domain.Inventory.Entities.PurchaseOrder> PurchaseOrders { get; set; } = new List<Domain.Inventory.Entities.PurchaseOrder>();
    
    public static bool NotExists(Supplier? supplier, NotificationContext notificationContext)
    {
        if (supplier != null) return false;
        notificationContext.AddNotification("Fornecedor", "O fornecedor não existe.");
        return true;
    }
}
