using Domain.Identity.Entities;
using Domain.Financial.Entities;
using Domain.Inventory.Entities;
using Domain.Business.Entities;
using Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.Shared.Abstractions;
using Flunt.Notifications;

namespace Infrastructure.Persistence;

public class CashRegisterDbContext(DbContextOptions<CashRegisterDbContext> options) : DbContext(options), IUnitOfWork
{
    // Identity
    public DbSet<User> Users { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<Customer> Customers { get; set; }

    // Security
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Financial
    public DbSet<FinancialConfiguration> FinancialConfigurations { get; set; }
    public DbSet<CashFlow> CashFlows { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    
    public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
    
    public DbSet<FinancialAccount> FinancialAccounts { get; set; }
    
    public DbSet<FinancialDocument> FinancialDocuments { get; set; }
    
    public DbSet<FinancialInstallment> FinancialInstallments { get; set; }
    
    public DbSet<FinancialCostCenterAllocation> FinancialCostCenterAllocations { get; set; }
    
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    
    public DbSet<JournalEntry> JournalEntries { get; set; }
    
    public DbSet<JournalEntryItem> JournalEntryItems { get; set; }
    
    public DbSet<BankStatementImport> BankStatementImports { get; set; }
    
    public DbSet<BankStatementItem> BankStatementItems { get; set; }

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
        modelBuilder.HasPostgresExtension("pg_trgm");

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
