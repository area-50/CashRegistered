using Flunt.Notifications;
using Shared.Validations;

namespace Shared.Abstractions;

public abstract class BaseEntity : GeneralValidator
{
    public int Id { get; protected set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
    public bool IsInvalid => _notifications.Any();
    
    protected void RegisterUpdate()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        RegisterUpdate();
    }
    
    public void Activate()
    {
        IsActive = true;
        RegisterUpdate();
    }

    protected void UpdateActivation(bool isActive)
    {
        if (isActive) Activate();
        else Deactivate();
    }

    protected void AddNotifications(IReadOnlyCollection<Notification> notifications)
    {
        _notifications.AddRange(notifications);
    }

    protected void ClearNotifications()
    {
        _notifications.Clear();
    }

    protected void AddNotification(string key, string message)
    {
        _notifications.Add(new Notification(key, message));
    }
} 