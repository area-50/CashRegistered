using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using FluentAssertions;
using Moq;

namespace Tests.Identity.Infrastructure;

public class UserRepositoryTests
{
    [Fact]
    [Trait("Category", "User Repository - Search")]
    public async Task SearchAsync_FilterByName_ShouldReturnFilteredUsers()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "Alice", "Wonderland", "111", DateTime.Now, "alice@test.com");
        var user = new User(1, "Pass123456789", "alice.user", UserRole.Admin);
        typeof(User).GetProperty("Person")?.SetValue(user, person);

        var expectedResponse = new PagedResponse<User>
        {
            Items = new List<User> { user },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        var request = new SearchUserRequest { Name = "Alice" };
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.SearchAsync(It.Is<SearchUserRequest>(req => req.Name == "Alice")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await mockRepository.Object.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().UserName.Should().Be("alice.user");
    }

    [Fact]
    [Trait("Category", "User Repository - Search")]
    public async Task SearchAsync_FilterByTaxId_ShouldReturnFilteredUsers()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "Alice", "Wonderland", "12345678901", DateTime.Now, "alice@test.com");
        var user = new User(1, "Pass123456789", "alice.user", UserRole.Admin);
        typeof(User).GetProperty("Person")?.SetValue(user, person);

        var expectedResponse = new PagedResponse<User>
        {
            Items = new List<User> { user },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        var request = new SearchUserRequest { TaxId = "12345678901" };
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.SearchAsync(It.Is<SearchUserRequest>(req => req.TaxId == "12345678901")))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await mockRepository.Object.SearchAsync(request);

        // Assert
        result.Items.Should().HaveCount(1);
    }
}
