using Domain.Identity.Entities;
using Domain.Financial.Entities;
using Domain.Inventory.Entities;
using Domain.Business.Entities;
using Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Abstractions;
using Flunt.Notifications;

namespace Infrastructure.Persistence;

public class CashRegisterDbContext(DbContextOptions<CashRegisterDbContext> options) : DbContext(options), IUnitOfWork
{
    // Identity
    public DbSet<User> Users { get; set; }
    public DbSet<Person> People { get; set; }

    // Security
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Financial
    public DbSet<CashFlow> CashFlows { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }

    // Inventory - Classificação e Produtos
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<UomConversion> UomConversions { get; set; }
    public DbSet<Product> Products { get; set; }

    // Inventory - Estoque e Movimentação
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockBalance> StockBalances { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<InventoryTransactionItem> InventoryTransactionItems { get; set; }


    // Inventory - Suprimentos e Compras
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    // Inventory - Consumo Interno
    public DbSet<InternalRequisition> InternalRequisitions { get; set; }
    public DbSet<InternalRequisitionItem> InternalRequisitionItems { get; set; }

    // Business / Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Notification>();

        // Aplica todas as configurações (Configurations) definidas neste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashRegisterDbContext).Assembly);
        
        // EF Core Global UTC Converter
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()),
            v => !v.HasValue ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(dateTimeConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableDateTimeConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public async Task<bool> CommitAsync() => await SaveChangesAsync() > 0;
}
