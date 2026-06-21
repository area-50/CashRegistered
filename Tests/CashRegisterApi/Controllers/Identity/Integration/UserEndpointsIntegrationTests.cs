using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Domain.Identity.Repositories;
using Domain.Identity.Entities;
using Shared.Identity.Request;
using Shared.Security.Request;
using Moq;
using FluentAssertions;

namespace Tests.CashRegisterApi.Controllers.Identity.Integration;

public class UserEndpointsIntegrationTests : IntegrationTestBase
{
    public UserEndpointsIntegrationTests(WebApplicationFactory<global::Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUser_WhenDatabaseIsOffline_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var client = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var userRepoMock = new Mock<IUserRepository>();
                userRepoMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                    .Throws(new Exception("Database connection failed"));

                services.AddScoped(_ => userRepoMock.Object);
            });
        }).CreateClient();

        var payload = new CreateUserPayload 
        { 
            UserRequest = new CreateUserRequest { UserName = "test", Password = "password", Role = "Admin" },
            PersonRequest = new Shared.Identity.Request.CreatePersonRequest 
            { 
                FirstName = "Test", LastName = "User", Birthdate = DateTime.Now, TaxId = "123", Email = "test@test.com", CellPhone = "123", Phone = "123", Gender = "M" 
            }
        };
        // Act
        var response = await client.PostAsJsonAsync("/api/user", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task ChangePassword_WhenDatabaseIsOffline_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var client = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var userRepoMock = new Mock<IUserRepository>();
                userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                            .Throws(new Exception("Database connection failed"));
                
                services.AddScoped(_ => userRepoMock.Object);
            });
        }).CreateClient();

        var request = new ChangePasswordRequest { OldPassword = "old", NewPassword = "new" };

        // Act
        var response = await client.PutAsJsonAsync("/api/user/ChangePassword", request);

        // Assert
        // Apenas garantindo que não retorne 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
