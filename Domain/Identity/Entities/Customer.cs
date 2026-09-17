using Domain.Shared.Abstractions;

namespace Domain.Identity.Entities;

public class Customer : BaseEntity
{
    public int PersonId { get; private set; }

    public decimal? CreditLimit { get; private set; }

    public string? Notes { get; private set; }

    public Person Person { get; private set; } = null!;

    protected Customer() { }

    public Customer(int personId, decimal? creditLimit = null, string? notes = null)
    {
        PersonId = personId;
        CreditLimit = creditLimit;
        Notes = notes;

        Validate();
    }

    public void Update(decimal? creditLimit, string? notes)
    {
        CreditLimit = creditLimit;
        Notes = notes;

        RegisterUpdate();
        Validate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (PersonId <= 0)
            AddNotification("Pessoa", "A pessoa vinculada é obrigatória.");

        if (CreditLimit.HasValue && CreditLimit < 0)
            AddNotification("LimiteCredito", "O limite de crédito não pode ser negativo.");
    }

    public static bool NotExists(Customer? customer, NotificationContext notificationContext)
    {
        if (customer != null) return false;
        notificationContext.AddNotification("Cliente", "O cliente não existe.");
        return true;
    }
}
