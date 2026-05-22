using Flunt.Br;
using Shared.Abstractions;
using Shared.Notifications;

namespace Domain.Inventory.Entities;

public class Product : BaseEntity
{
    public Product(
        string sku,
        string name,
        int categoryId,
        int baseUomId,
        ICollection<Tag> tags,
        string? description = null,
        string? ncmCode = null
    )
    {
        Sku = sku;
        Name = name;
        CategoryId = categoryId;
        BaseUomId = baseUomId;
        Description = description;
        NcmCode = ncmCode;
        Tags = tags;
        
        EntityValidate();
    }

    protected Product() { }

    public string Sku { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? NcmCode { get; set; }
    
    public int CategoryId { get; set; }
    
    public Category Category { get; set; }
    
    public int BaseUomId { get; set; }
    
    public UnitOfMeasure BaseUom { get; set; }
    
    public decimal AverageCost { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    
    private void EntityValidate()
    {
        var contract = new Contract()
            .Requires()
            .IsNotNullOrEmpty(Name, "Nome", "O nome da produto é obrigatório.")
            .IsGreaterThan(Name, 3, "Nome", "O nome deve conter mais que 3 caracteres.")
            .IsNotNullOrEmpty(Sku, "Sku", "Sku é obrigatório.")
            .IsGreaterThan(CategoryId, 0, "Categoria", "Categoria é obrigatória.")
            .IsGreaterThan(BaseUomId, 0, "Unidade de medida", "Unidade de medida é obrigatória.");
        AddNotifications(contract.Notifications);
    }

    public static bool NotExists(Product? tag, NotificationContext notificationContext)
    {
        if (tag != null) return false;
        notificationContext.AddNotification("Produto", "O produto não existe.");
        return true;
    }
}
