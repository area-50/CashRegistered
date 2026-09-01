using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Infrastructure.Identity.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Tests.Identity.Infrastructure;

public class PersonRepositoryTests
{
    private CashRegisterDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<CashRegisterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CashRegisterDbContext(options);
    }

    [Fact]
    [Trait("Category", "Person Repository - Query")]
    public async Task GetPersonByTaxId_WhenExists_ShouldReturnPerson()
    {
        // Arrange
        using var context = GetDbContext();
        var taxId = "12345678901";
        var person = new Person(PersonType.Physical, "Test", "User", taxId, DateTime.Now, "test@test.com");
        
        await context.People.AddAsync(person);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetPersonByTaxId(taxId);

        // Assert
        result.Should().NotBeNull();
        result!.TaxId.Should().Be(taxId);
    }

    [Fact]
    [Trait("Category", "Person Repository - Query")]
    public async Task GetPersonByEmail_WhenExists_ShouldReturnPerson()
    {
        // Arrange
        using var context = GetDbContext();
        var email = "unique@test.com";
        var person = new Person(PersonType.Physical, "Test", "User", "123", DateTime.Now, email);
        
        await context.People.AddAsync(person);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetPersonByEmail(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }
}
