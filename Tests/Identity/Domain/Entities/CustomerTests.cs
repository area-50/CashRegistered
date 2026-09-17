using Domain.Identity.Entities;
using Domain.Shared.Abstractions;
using Xunit;

namespace Tests.Identity.Domain.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateCustomer()
    {
        // Act
        var customer = new Customer(personId: 1, creditLimit: 5000.00m, notes: "Cliente Preferencial");

        // Assert
        Assert.False(customer.IsInvalid);
        Assert.Equal(1, customer.PersonId);
        Assert.Equal(5000.00m, customer.CreditLimit);
        Assert.Equal("Cliente Preferencial", customer.Notes);
    }

    [Fact]
    public void Constructor_InvalidPersonId_ShouldAddNotification()
    {
        // Act
        var customer = new Customer(personId: 0);

        // Assert
        Assert.True(customer.IsInvalid);
        Assert.Contains(customer.Notifications, n => n.Key == "Pessoa");
    }

    [Fact]
    public void Constructor_NegativeCreditLimit_ShouldAddNotification()
    {
        // Act
        var customer = new Customer(personId: 1, creditLimit: -100.00m);

        // Assert
        Assert.True(customer.IsInvalid);
        Assert.Contains(customer.Notifications, n => n.Key == "LimiteCredito");
    }

    [Fact]
    public void NotExists_NullCustomer_ShouldReturnTrueAndAddNotification()
    {
        // Arrange
        var context = new NotificationContext();

        // Act
        var result = Customer.NotExists(null, context);

        // Assert
        Assert.True(result);
        Assert.True(context.IsInvalid);
        Assert.Contains(context.Notifications, n => n.Key == "Cliente" && n.Message.Contains("não existe"));
    }
}
