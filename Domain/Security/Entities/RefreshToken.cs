using Domain.Identity.Entities;
using Flunt.Validations;
using Shared.Abstractions;

namespace Domain.Security.Entities;

public class RefreshToken : BaseEntity
{
    public RefreshToken(string token, int userId, DateTime expiresAt)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;

        Validate();
    }

    protected RefreshToken() { } // For EF Core

    public string Token { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    // BaseEntity já possui IsActive = true por padrão. Se falso, foi revogado via Deactivate().
    public bool IsValid => IsActive && !IsExpired;

    public void Revoke()
    {
        Deactivate();
    }

    private void Validate()
    {
        var contract = new Contract<Flunt.Notifications.Notification>()
            .Requires()
            .IsNotNullOrEmpty(Token, "Token", "O token não pode ser vazio.")
            .IsGreaterThan(UserId, 0, "UserId", "O usuário deve ser válido.");

        AddNotifications(contract.Notifications);
    }
}
