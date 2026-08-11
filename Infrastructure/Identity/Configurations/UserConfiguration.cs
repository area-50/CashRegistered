using Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Identity.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(user => user.Id);

        builder.Property(user => user.HashedPassword)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(user => user.UserName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(user => user.UserName)
            .IsUnique();

        builder.Property(user => user.UserRole)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Ignore(user => user.RawPassword);

        builder.HasOne(user => user.Person)
            .WithOne()
            .HasForeignKey<User>(user => user.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(user => user.Timezone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("America/Sao_Paulo");
    }
}