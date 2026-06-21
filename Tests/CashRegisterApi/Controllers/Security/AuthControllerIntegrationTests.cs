using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Domain.Identity.Repositories;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;


namespace Tests.CashRegisterApi.Controllers.Security;

public class AuthControllerIntegrationTests(WebApplicationFactory<global::Program> factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_WhenDatabaseIsOffline_ShouldReturnServiceUnavailable()
    {
        // Arrange
        var client = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Simulate Infrastructure failure by mocking repository to throw exception
                var userRepoMock = new Mock<IUserRepository>();
                userRepoMock.Setup(x => x.GetUserByUserName(It.IsAny<string>()))
                            .Throws(new Exception("Database connection failed"));
                
                services.AddScoped(_ => userRepoMock.Object);
            });
        }).CreateClient();

        var request = new { UserName = "testUser", Password = "password" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth", request);

        // Assert
        // This is what we expect to fail based on current Middleware implementation (it returns 500)
        // We want to update it to return 503 later.
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
