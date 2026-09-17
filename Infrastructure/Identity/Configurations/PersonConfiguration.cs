using Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Identity.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        
        builder.HasKey(person => person.Id);
        
        builder.Property(person => person.PersonType)
            .IsRequired()
            .HasConversion<int>();

        builder.OwnsOne(person => person.Name, name =>
        {
            name.Property(u => u.FirstName).IsRequired().HasMaxLength(255);
            name.Property(u => u.LastName).IsRequired(false).HasMaxLength(255);
        });

        builder.Property(person => person.TaxId)
            .IsRequired()
            .HasMaxLength(14); // CPF or CNPJ
        
        builder.Property(person => person.TradeName)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(person => person.StateRegistration)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(person => person.MunicipalRegistration)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(person => person.Gender)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(person => person.Birthdate)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        
        builder.Property(person => person.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(person => person.Phone)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(person => person.CellPhone)
            .IsRequired(false)
            .HasMaxLength(20);
        
        builder.HasIndex(person => person.Email).IsUnique();
        builder.HasIndex(person => person.TaxId).IsUnique();
    }
}
