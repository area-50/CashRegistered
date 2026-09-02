using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class CostCenter : BaseEntity
{
    public string Name { get; private set; } = null!;
    public int ManagerId { get; private set; }
    public User? Manager { get; private set; }

    protected CostCenter() { }

    public CostCenter(string name, int managerId)
    {
        Name = name;
        ManagerId = managerId;

        Validate();
    }

    public void Update(string name, int managerId)
    {
        Name = name;
        ManagerId = managerId;

        RegisterUpdate();
        Validate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(Name))
            AddNotification("Nome", "O nome do centro de custo é obrigatório.");
        else if (Name.Length > 150)
            AddNotification("Nome", "O nome do centro de custo não pode exceder 150 caracteres.");

        if (ManagerId <= 0)
            AddNotification("GerenteId", "O gerente responsável é obrigatório.");
    }

    public static bool NotExists(CostCenter? costCenter, NotificationContext notificationContext)
    {
        if (costCenter != null) return false;
        notificationContext.AddNotification("CentroDeCusto", "O centro de custo não existe.");
        return true;
    }
}
