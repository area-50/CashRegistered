using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Financial.Configurations;

public class ChartOfAccountsConfiguration : IEntityTypeConfiguration<ChartOfAccounts>
{
    public void Configure(EntityTypeBuilder<ChartOfAccounts> builder)
    {
        builder.ToTable("ChartOfAccounts");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        
        builder.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(x => x.Nature).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(x => x.IsSynthetic).IsRequired().HasDefaultValue(true);
        
        builder.Property(x => x.AllowPosting).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.ChildAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new
            {
                Id = 1,
                Code = "1",
                Name = "ATIVO",
                AccountType = AccountType.Asset,
                Nature = AccountNature.Debit,
                ParentAccountId = (int?)null,
                IsSynthetic = true,
                AllowPosting = false,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = 2,
                Code = "2",
                Name = "PASSIVO",
                AccountType = AccountType.Liability,
                Nature = AccountNature.Credit,
                ParentAccountId = (int?)null,
                IsSynthetic = true,
                AllowPosting = false,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}

public class FinancialAccountConfiguration : IEntityTypeConfiguration<FinancialAccount>
{
    public void Configure(EntityTypeBuilder<FinancialAccount> builder)
    {
        builder.ToTable("FinancialAccounts");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        
        builder.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(30);
        
        builder.Property(x => x.BankCode).HasMaxLength(20).IsRequired(false);
        
        builder.Property(x => x.BankName).HasMaxLength(100).IsRequired(false);
        
        builder.Property(x => x.AgencyNumber).HasMaxLength(20).IsRequired(false);
        
        builder.Property(x => x.AccountNumber).HasMaxLength(30).IsRequired(false);
        
        builder.Property(x => x.InitialBalance).HasPrecision(18, 2);
        
        builder.Property(x => x.CurrentBalance).HasPrecision(18, 2);
        
        builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("BRL");

        builder.HasOne(x => x.ChartOfAccounts)
            .WithMany()
            .HasForeignKey(x => x.ChartOfAccountsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FinancialDocumentConfiguration : IEntityTypeConfiguration<FinancialDocument>
{
    public void Configure(EntityTypeBuilder<FinancialDocument> builder)
    {
        builder.ToTable("FinancialDocuments");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(x => x.DocumentNumber).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.InterestAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.FineAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.FineRate).HasPrecision(5, 2).HasDefaultValue(0m);
        
        builder.Property(x => x.InterestDailyRate).HasPrecision(7, 4).HasDefaultValue(0m);
        
        builder.Property(x => x.NetAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        
        builder.Property(x => x.Notes).HasMaxLength(500).IsRequired(false);

        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ChartOfAccounts)
            .WithMany().
            HasForeignKey(x => x.ChartOfAccountsId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ApprovalUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovalUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FinancialInstallmentConfiguration : IEntityTypeConfiguration<FinancialInstallment>
{
    public void Configure(EntityTypeBuilder<FinancialInstallment> builder)
    {
        builder.ToTable("FinancialInstallments");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.InterestAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.FineAmount).HasPrecision(18, 2);
        
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.FinancialDocument)
            .WithMany(x => x.Installments)
            .HasForeignKey(x => x.FinancialDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FinancialCostCenterAllocationConfiguration : IEntityTypeConfiguration<FinancialCostCenterAllocation>
{
    public void Configure(EntityTypeBuilder<FinancialCostCenterAllocation> builder)
    {
        builder.ToTable("FinancialCostCenterAllocations");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Percentage).HasPrecision(5, 2);
        
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.FinancialDocument)
            .WithMany(x => x.Allocations)
            .HasForeignKey(x => x.FinancialDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);
        
        builder.Property(x => x.DiscountApplied).HasPrecision(18, 2);
        
        builder.Property(x => x.InterestApplied).HasPrecision(18, 2);
        
        builder.Property(x => x.FineApplied).HasPrecision(18, 2);
        
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        
        builder.Property(x => x.TransactionReceiptNumber).HasMaxLength(100).IsRequired(false);
        
        builder.Property(x => x.Notes).HasMaxLength(500).IsRequired(false);

        builder.HasOne(x => x.FinancialInstallment)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.FinancialInstallmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Description).IsRequired().HasMaxLength(255);
        
        builder.Property(x => x.SourceModule).IsRequired().HasMaxLength(50);
        
        builder.Property(x => x.ReferenceDocument).HasMaxLength(100).IsRequired(false);
        
        builder.Property(x => x.TotalDebit).HasPrecision(18, 2);
        
        builder.Property(x => x.TotalCredit).HasPrecision(18, 2);
        
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.ReversedJournalEntry)
            .WithMany()
            .HasForeignKey(x => x.ReversedJournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JournalEntryItemConfiguration : IEntityTypeConfiguration<JournalEntryItem>
{
    public void Configure(EntityTypeBuilder<JournalEntryItem> builder)
    {
        builder.ToTable("JournalEntryItems");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EntryType).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        
        builder.Property(x => x.Memo).HasMaxLength(255).IsRequired(false);

        builder.HasOne(x => x.JournalEntry)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankStatementImportConfiguration : IEntityTypeConfiguration<BankStatementImport>
{
    public void Configure(EntityTypeBuilder<BankStatementImport> builder)
    {
        builder.ToTable("BankStatementImports");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);

        builder.HasOne(x => x.FinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankStatementItemConfiguration : IEntityTypeConfiguration<BankStatementItem>
{
    public void Configure(EntityTypeBuilder<BankStatementItem> builder)
    {
        builder.ToTable("BankStatementItems");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Description).IsRequired().HasMaxLength(255);
        
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        
        builder.Property(x => x.FitId).IsRequired().HasMaxLength(100);
        
        builder.Property(x => x.CheckNumber).HasMaxLength(50).IsRequired(false);
        
        builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(20);
        
        builder.Property(x => x.ReconciliationStatus).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => x.FitId).IsUnique();

        builder.HasOne(x => x.BankStatementImport)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BankStatementImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PaymentTransaction)
            .WithMany()
            .HasForeignKey(x => x.PaymentTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
