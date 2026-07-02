using Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Inventory.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        
        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.HexColor).HasMaxLength(7);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class UomConversionConfiguration : IEntityTypeConfiguration<UomConversion>
{
    public void Configure(EntityTypeBuilder<UomConversion> builder)
    {
        builder.ToTable("UomConversions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Multiplier).HasPrecision(18, 6);

        builder.HasOne(x => x.FromUom).WithMany().HasForeignKey(x => x.FromUomId);
        builder.HasOne(x => x.ToUom).WithMany().HasForeignKey(x => x.ToUomId);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sku).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.AverageCost).HasPrecision(18, 2);
        
        builder.HasIndex(x => x.Sku).IsUnique();

        builder.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
        builder.HasOne(x => x.BaseUom).WithMany().HasForeignKey(x => x.BaseUomId);

        builder.HasMany(x => x.Tags)
            .WithMany(y => y.Products)
            .UsingEntity<Dictionary<string, object>>(
               "ProductTags",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId")
            );
    }
}

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AvailableQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 4);

        builder.HasOne(x => x.Product).WithMany(x => x.StockBalances).HasForeignKey(x => x.ProductId);
        builder.HasOne(x => x.Warehouse).WithMany(x => x.StockBalances).HasForeignKey(x => x.WarehouseId);
        
        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
    }
}

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
        
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}

public class InventoryTransactionItemConfiguration : IEntityTypeConfiguration<InventoryTransactionItem>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionItem> builder)
    {
        builder.ToTable("InventoryTransactionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TransactionQuantity).HasPrecision(18, 4);
        builder.Property(x => x.BaseQuantity).HasPrecision(18, 4);

        builder.HasOne(x => x.Transaction).WithMany(x => x.Items).HasForeignKey(x => x.TransactionId);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        builder.HasOne(x => x.SourceWarehouse).WithMany().HasForeignKey(x => x.SourceWarehouseId);
        builder.HasOne(x => x.DestinationWarehouse).WithMany().HasForeignKey(x => x.DestinationWarehouseId);
        builder.HasOne(x => x.Uom).WithMany().HasForeignKey(x => x.UomId);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Person).WithOne().HasForeignKey<Supplier>(x => x.PersonId);
    }
}

public class PurchaseRequisitionConfiguration : IEntityTypeConfiguration<PurchaseRequisition>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisition> builder)
    {
        builder.ToTable("PurchaseRequisitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}

public class PurchaseRequisitionItemConfiguration : IEntityTypeConfiguration<PurchaseRequisitionItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisitionItem> builder)
    {
        builder.ToTable("PurchaseRequisitionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestedQuantity).HasPrecision(18, 4);

        builder.HasOne(x => x.Requisition).WithMany(x => x.Items).HasForeignKey(x => x.RequisitionId);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        builder.HasOne(x => x.Uom).WithMany().HasForeignKey(x => x.UomId);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.HasOne(x => x.Supplier).WithMany(x => x.PurchaseOrders).HasForeignKey(x => x.SupplierId);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        builder.HasOne(x => x.Uom).WithMany().HasForeignKey(x => x.UomId);
        builder.HasOne(x => x.RequisitionItem).WithMany().HasForeignKey(x => x.RequisitionItemId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId);
    }
}

public class InternalRequisitionConfiguration : IEntityTypeConfiguration<InternalRequisition>
{
    public void Configure(EntityTypeBuilder<InternalRequisition> builder)
    {
        builder.ToTable("InternalRequisitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId);
    }
}

public class InternalRequisitionItemConfiguration : IEntityTypeConfiguration<InternalRequisitionItem>
{
    public void Configure(EntityTypeBuilder<InternalRequisitionItem> builder)
    {
        builder.ToTable("InternalRequisitionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.FulfilledQuantity).HasPrecision(18, 4);

        builder.HasOne(x => x.InternalRequisition).WithMany(x => x.Items).HasForeignKey(x => x.InternalRequisitionId);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        builder.HasOne(x => x.Uom).WithMany().HasForeignKey(x => x.UomId);
    }
}

public class InventoryRequisitionConfiguration : IEntityTypeConfiguration<InventoryRequisition>
{
    public void Configure(EntityTypeBuilder<InventoryRequisition> builder)
    {
        builder.ToTable("InventoryRequisitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginModule).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Notes).HasMaxLength(500).IsRequired(false);
    }
}

public class InventoryRequisitionItemConfiguration : IEntityTypeConfiguration<InventoryRequisitionItem>
{
    public void Configure(EntityTypeBuilder<InventoryRequisitionItem> builder)
    {
        builder.ToTable("InventoryRequisitionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);

        builder.HasOne(x => x.Requisition)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.InventoryRequisitionId);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);
    }
}
