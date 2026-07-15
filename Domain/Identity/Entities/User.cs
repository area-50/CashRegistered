using Domain.Identity.Enums;
using Domain.Security.Interfaces;
using Domain.Financial.Entities;
using Shared.Abstractions;
using Shared.Notifications;
using Flunt.Notifications;
using Flunt.Validations;

namespace Domain.Identity.Entities;

public class User : BaseEntity
{
   

    public User(
        int personId,
        string rawPassword,
        string userName,
        UserRole userRole
    )
    {
        PersonId = personId;
        RawPassword = rawPassword;
        UserName = userName;
        UserRole = userRole;

        Validate();
    }

    protected User() { }

    public int PersonId { get; set; }
    
    public string HashedPassword { get; set; } = string.Empty;
    
    public string RawPassword { get; set; }
    
    public string UserName { get; set; }
    
    public UserRole UserRole { get; set; }
    
    public CashFlow? CashFlow { get; private set; }
    
    public Person Person { get; private set; }

    private void Validate()
    {
        var contract = new Contract<Notification>()
            .Requires()
            .IsNotNullOrEmpty(
                UserName,
                "Nome de usuário",
                "O nome de usuário é obrigatório."
            )
            .IsGreaterThan(
                UserName?.Length ?? 0,
                3, "Nome de usuário",
                "O nome de usuário deve ter mais que 3 caracteres."
            )
            .IsNotNullOrEmpty(
                RawPassword,
                "Senha",
                "A senha é obrigatória."
            )
            .IsGreaterOrEqualsThan(
                RawPassword?.Length ?? 0,
                12, "Senha",
                "A senha deve ter pelo menos 12 caracteres."
            );
        AddNotifications(contract.Notifications);
    }

    public void ValidateUniqueUser(bool userNameExists, bool personAlreadyHasUser)
    {
        if (userNameExists)
            AddNotification("UserName", "Este nome de usuário já está em uso.");

        if (personAlreadyHasUser)
            AddNotification("Person", "Esta pessoa já possui um usuário vinculado.");
    }

    public void HashPassword(IPasswordHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(RawPassword)) return;
        HashedPassword = hasher.HashPassword(RawPassword);
    }

    public void UpdatePassword(string newPassword, IPasswordHasher hasher)
    {
        RawPassword = newPassword;
        Validate();
        if (!IsInvalid)
            HashPassword(hasher);
    }

    public bool AuthenticatePassword(IPasswordHasher hasher, string password)
    {
        if (hasher.VerifyHash(password, HashedPassword))
            return true;

        AddNotification("Login", "Usuário ou senha inválidos.");
        return false;
    }

    public void ValidateUserHasCashFlow()
    {
        if (CashFlow == null)
            AddNotification("CashFlow", "O usuário não possui um fluxo de caixa cadastrado.");
    }

    public static void ValidateUserExists(User? targetUser, NotificationContext notificationContext)
    {
        if (targetUser == null)
            notificationContext.AddNotification("Usuário", "O usuário não existe.");
    }
    
    public static void ValidateUserLoginExists(User? targetUser, NotificationContext notificationContext)
    {
        if (targetUser == null)
            notificationContext.AddNotification("Login", "Usuário ou senha inválidos.");
    }
}
