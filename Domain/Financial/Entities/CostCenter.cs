using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class CostCenter : BaseEntity
{
    public CostCenter(string name, int managerId)
    {
        Name = name;
        ManagerId = managerId;
    }

    protected CostCenter() { }

    public string Name { get; set; }
    
    public int ManagerId { get; set; }
    
    public User Manager { get; set; }

    public static bool NotExists(CostCenter? costCenter, NotificationContext notificationContext)
    {
        if (costCenter != null) return false;
        notificationContext.AddNotification("CentroDeCusto", "O centro de custo não existe.");
        return true;
    }
}
