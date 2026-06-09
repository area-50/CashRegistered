using Domain.Inventory.Entities;
using FluentAssertions;
using Xunit;

namespace Tests.Domain.Inventory.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var product = new Product("SKU1", "Name1", 1, 1, new List<Tag>(), "Desc", "NCM1");

        product.Sku.Should().Be("SKU1");
        product.Name.Should().Be("Name1");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var product = new Product("SKU1", "Name1", 1, 1, new List<Tag>(), "Desc", "NCM1");

        product.Deactivate();

        product.IsActive.Should().BeFalse();
    }
}
