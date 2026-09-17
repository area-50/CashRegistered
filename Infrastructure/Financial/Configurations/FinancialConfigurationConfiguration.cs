using Domain.Financial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Financial.Configurations;

public class FinancialConfigurationConfiguration : IEntityTypeConfiguration<FinancialConfiguration>
{
    public void Configure(EntityTypeBuilder<FinancialConfiguration> builder)
    {
        builder.ToTable("FinancialConfigurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApprovalThresholdAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(x => x.EnableApprovalWorkflow).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.AllowAutoApprovalForManagers).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DefaultInterestDailyRate).HasPrecision(7, 4).HasDefaultValue(0m);
        builder.Property(x => x.DefaultFineRate).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

        builder.HasData(
            new
            {
                Id = 1,
                ApprovalThresholdAmount = 0m,
                EnableApprovalWorkflow = false,
                AllowAutoApprovalForManagers = false,
                DefaultInterestDailyRate = 0m,
                DefaultFineRate = 0m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
