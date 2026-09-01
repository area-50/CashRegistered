using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.ValueObjects;
using FluentAssertions;

namespace Tests.Identity.Domain.Entities;

public class PersonTests
{
    [Fact]
    [Trait("Category", "Person Domain - Validation")]
    public void Constructor_WithValidPhysicalPerson_ShouldCreateAndBeValid()
    {
        // Arrange & Act
        var person = new Person(
            PersonType.Physical, 
            "Fernando", 
            "Mendes", 
            "12345678901", 
            DateTime.Now.AddYears(-30), 
            "fernando@test.com"
        );

        // Assert
        person.IsInvalid.Should().BeFalse();
        person.Name.FirstName.Should().Be("Fernando");
        person.Name.LastName.Should().Be("Mendes");
        person.PersonType.Should().Be(PersonType.Physical);
        person.CellPhone.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Person Domain - Validation")]
    public void Constructor_WithValidLegalPerson_ShouldCreateAndBeValid()
    {
        // Arrange & Act
        var person = new Person(
            PersonType.Legal, 
            "Company", 
            "Inc", 
            "12345678000199", 
            DateTime.Now.AddYears(-5), 
            "contact@company.com",
            tradeName: "Best Company"
        );

        // Assert
        person.IsInvalid.Should().BeFalse();
        person.TradeName.Should().Be("Best Company");
    }

    public static IEnumerable<object[]> InvalidPersonData =>
        new List<object[]>
        {
            new object[] { "", "fernando@test.com", "CPF/CNPJ", "O CPF/CNPJ é obrigatório." },
            new object[] { "12345678901", "invalid-email", "E-mail", "E-mail inválido." }
        };

    [Theory]
    [Trait("Category", "Person Domain - Validation")]
    [MemberData(nameof(InvalidPersonData))]
    public void Constructor_WithInvalidParameters_ShouldHaveNotifications(
        string taxId, 
        string email, 
        string expectedKey, 
        string expectedMessage)
    {
        // Arrange & Act
        var person = new Person(PersonType.Physical, "FirstName", "LastName", taxId, DateTime.Now, email);

        // Assert
        person.IsInvalid.Should().BeTrue();
        person.Notifications.Should().Contain(n => n.Key == expectedKey && n.Message == expectedMessage);
    }

    [Fact]
    [Trait("Category", "Person Domain - Validation")]
    public void Constructor_WithLegalPersonMissingTradeName_ShouldHaveNotification()
    {
        // Arrange & Act
        var person = new Person(PersonType.Legal, "Company", "Inc", "12345678000199", DateTime.Now, "contact@company.com", tradeName: "");

        // Assert
        person.IsInvalid.Should().BeTrue();
        person.Notifications.Should().Contain(n => n.Key == "Nome Fantasia" && n.Message == "O Nome Fantasia é obrigatório para Pessoa Jurídica.");
    }

    [Fact]
    [Trait("Category", "Person Domain - Address")]
    public void AddAddress_ShouldAddAddressToCollectionAndRegisterUpdate()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "FirstName", "LastName", "12345678901", DateTime.Now, "test@test.com");
        var address = new Address("12345678", "Street", "123", "Apt 1", "District", "City", "ST", AddressType.Home, true);

        // Act
        person.AddAddress(address);

        // Assert
        person.Addresses.Should().Contain(address);
        person.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Person Domain - Validation")]
    public void ValidateUniquePerson_WhenTaxIdExists_ShouldAddNotification()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "FirstName", "LastName", "12345678901", DateTime.Now, "test@test.com");

        // Act
        person.ValidateUniquePerson(taxIdExists: true);

        // Assert
        person.IsInvalid.Should().BeTrue();
        person.Notifications.Should().Contain(n => n.Key == "CPF/CNPJ" && n.Message == "Já existe uma pessoa cadastrada com este CPF/CNPJ.");
    }

    [Fact]
    [Trait("Category", "Person Domain - Validation")]
    public void ValidatePersonExists_WhenNull_ShouldAddNotificationToContext()
    {
        // Arrange
        Person? person = null;
        var notificationContext = new NotificationContext();

        // Act
        Person.ValidatePersonExists(person, notificationContext);

        // Assert
        notificationContext.IsInvalid.Should().BeTrue();
        notificationContext.Notifications.Should().Contain(n => n.Key == "Pessoa" && n.Message == "A pessoa não existe.");
    }
}
