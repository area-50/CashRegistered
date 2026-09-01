using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Inventory.Entities;

public class Supplier : BaseEntity
{
    public Supplier(int personId)
    {
        PersonId = personId;
    }

    protected Supplier() { }

    public int PersonId { get; set; }
    
    public Person Person { get; set; }
    
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public static bool NotExists(Supplier? supplier, NotificationContext notificationContext)
    {
        if (supplier != null) return false;
        notificationContext.AddNotification("Fornecedor", "O fornecedor não existe.");
        return true;
    }
}
