using Flunt.Notifications;

namespace Domain.Shared.Notifications;

public class NotificationContext : Notifiable<Notification>
{
    public bool IsInvalid => !IsValid;
}
