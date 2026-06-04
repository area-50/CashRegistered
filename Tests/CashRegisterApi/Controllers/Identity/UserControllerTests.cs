using CashRegister.Controllers.Identity;
using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Shared.Response;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.CashRegisterApi.Controllers.Identity;

public class UserControllerTests
{
    private readonly Mock<IUserUseCase> _userUseCaseMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userUseCaseMock = new Mock<IUserUseCase>();
        _controller = new UserController(_userUseCaseMock.Object);
    }

    [Fact]
    [Trait("Category", "User Controller - Create")]
    public async Task CreateUser_ShouldReturnCreated()
    {
        // Arrange
        var payload = new CreateUserPayload 
        { 
            UserRequest = new CreateUserRequest { UserName = "test" },
            PersonRequest = null 
        };
        var response = new CreateResponse { Id = 1 };
        _userUseCaseMock.Setup(x => x.CreateUser(payload.UserRequest, payload.PersonRequest)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateUser(payload);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        _userUseCaseMock.Verify(x => x.CreateUser(payload.UserRequest, payload.PersonRequest), Times.Once);
    }

    [Fact]
    [Trait("Category", "User Controller - Query")]
    public async Task Search_ShouldReturnOkWithData()
    {
        // Arrange
        var request = new SearchUserRequest { Name = "test" };
        var response = new PagedResponse<GetAllUsersResponse> { Items = new List<GetAllUsersResponse>() };
        _userUseCaseMock.Setup(x => x.SearchUsers(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.Search(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    [Trait("Category", "User Controller - Action")]
    public async Task DeactivateUser_ShouldReturnOk()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _controller.DeactivateUser(id);

        // Assert
        result.Should().BeOfType<OkResult>();
        _userUseCaseMock.Verify(x => x.DeactivateUser(id), Times.Once);
    }

    [Fact]
    [Trait("Category", "User Controller - Security")]
    public async Task ChangePassword_WithValidUser_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var request = new Shared.Security.Request.ChangePasswordRequest { OldPassword = "A", NewPassword = "B" };
        
        var claims = new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        _userUseCaseMock.Verify(x => x.ChangePassword(userId, request), Times.Once);
    }
}
