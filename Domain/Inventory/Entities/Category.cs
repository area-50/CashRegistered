using Flunt.Notifications;
using Flunt.Validations;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class Category : BaseEntity
{
    public Category(string name, int? parentCategoryId = null)
    {
        Name = name;
        ParentCategoryId = parentCategoryId;

        EntityValidate();
    }

    protected Category() { }

    public string Name { get; set; }
    
    public int? ParentCategoryId { get; set; }
    
    public Category? ParentCategory { get; set; }
    
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    
    public ICollection<Product> Products { get; set; } = new List<Product>();

    private void EntityValidate()
    {
        var contract = new Contract<Notification>()
            .Requires()
            .IsNotNullOrWhiteSpace(Name, "Categoria", "O nome é obrigatório.");
        if (ParentCategoryId != null)
            contract.IsGreaterThan(
                ParentCategoryId.Value, 0, "Categoria", "A categoria não existe."
            );
        AddNotifications(contract.Notifications);
    }

    public void Update(string name, int? parentCategoryId, bool isActive)
    {
        Name = name;
        ParentCategoryId = parentCategoryId;
        
        UpdateActivation(isActive);
        EntityValidate();
        RegisterUpdate();
    }

    public static bool NotExists(Category? category, NotificationContext notificationContext)
    {
        if (category != null) return false;
        notificationContext.AddNotification("Categoria", "A categoria não existe.");
        return true;
    }
}
