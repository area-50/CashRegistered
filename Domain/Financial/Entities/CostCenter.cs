using Domain.Identity.Entities;
using Shared.Abstractions;

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
}
