using Application.Identity.Interfaces;
using Application.Identity.UseCases;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using Domain.Security.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Identity.Request;
using Shared.Notifications;
using Shared.Security.Request;

namespace Tests.Application.Identity.UseCases;

public class UserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IPersonUseCase> _personUseCaseMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly NotificationContext _notificationContext;
    private readonly UserUseCase _userUseCase;

    public UserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _personUseCaseMock = new Mock<IPersonUseCase>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _notificationContext = new NotificationContext();

        _userUseCase = new UserUseCase(
            _userRepositoryMock.Object,
            _personRepositoryMock.Object,
            _personUseCaseMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _notificationContext
        );
    }

    private CreateUserRequest CreateValidUserRequest() => new()
    {
        UserName = "valid.user",
        Password = "StrongPassword123",
        Role = "Admin",
        PersonId = 1
    };

    [Fact]
    [Trait("Category", "User Application - Create")]
    public async Task CreateUser_WithExistingPersonId_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var request = CreateValidUserRequest();
        var person = new Person(PersonType.Physical, "FirstName", "LastName", "12345678901", DateTime.Now.AddYears(-20), "email@test.com");
        
        _personRepositoryMock.Setup(x => x.GetByIdAsync(request.PersonId!.Value))
            .ReturnsAsync(person);
        
        _userRepositoryMock.Setup(x => x.GetUserByUserName(request.UserName))
            .ReturnsAsync((User?)null);
        
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var response = await _userUseCase.CreateUser(request, null);

        // Assert
        _notificationContext.IsInvalid.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "User Application - Create")]
    public async Task CreateUser_WithNewPersonData_ShouldCallPersonUseCaseAndCreateUser()
    {
        // Arrange
        var userRequest = CreateValidUserRequest();
        userRequest.PersonId = 0;
        var personRequest = new CreatePersonRequest { FirstName = "New", LastName = "Person" };
        
        _personUseCaseMock.Setup(x => x.CreatePerson(It.IsAny<CreatePersonRequest>()))
            .ReturnsAsync(new Shared.Response.CreateResponse { Id = 2 });

        // Act
        await _userUseCase.CreateUser(userRequest, personRequest);

        // Assert
        _personUseCaseMock.Verify(x => x.CreatePerson(It.IsAny<CreatePersonRequest>()), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "User Application - Deactivate")]
    public async Task DeactivateUser_WhenUserExists_ShouldDeactivateAndSave()
    {
        // Arrange
        var userId = 1;
        var user = new User(1, "StrongPassword123", "user.to.deactivate", UserRole.Logistics);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _userUseCase.DeactivateUser(userId);

        // Assert
        user.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "User Application - Deactivate")]
    public async Task DeactivateUser_WhenUserDoesNotExist_ShouldAddNotification()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        await _userUseCase.DeactivateUser(userId);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "User" && n.Message == "O usuário não existe.");
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    [Trait("Category", "User Application - Create")]
    public async Task CreateUser_WithExistingUsername_ShouldAddNotification()
    {
        // Arrange
        var request = CreateValidUserRequest();
        var person = new Person(PersonType.Physical, "FirstName", "LastName", "12345678901", DateTime.Now.AddYears(-20), "email@test.com");
        var existingUser = new User(1, "Pass", "valid.user", UserRole.Admin);

        _personRepositoryMock.Setup(x => x.GetByIdAsync(request.PersonId!.Value)).ReturnsAsync(person);
        _userRepositoryMock.Setup(x => x.GetUserByUserName(request.UserName)).ReturnsAsync(existingUser);
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        await _userUseCase.CreateUser(request, null);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "UserName" && n.Message == "Este nome de usuário já está em uso.");
    }

    [Fact]
    [Trait("Category", "User Application - Password")]
    public async Task ChangePassword_WithWrongOldPassword_ShouldAddNotification()
    {
        // Arrange
        var userId = 1;
        var request = new ChangePasswordRequest { OldPassword = "WrongPassword", NewPassword = "NewStrongPassword123" };
        var user = new User(1, "CorrectPassword", "user.test", UserRole.Admin);
        user.HashedPassword = "hashed_correct_password";

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyHash(request.OldPassword, user.HashedPassword)).Returns(false);

        // Act
        await _userUseCase.ChangePassword(userId, request);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Login" && n.Message == "Usuário ou senha inválidos.");
    }

    [Fact]
    [Trait("Category", "User Application - Query")]
    public async Task GetValidUserById_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var user = new User(1, "Pass", "user.test", UserRole.Admin);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userUseCase.GetValidUserById(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserName.Should().Be("user.test");
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "User Application - Query")]
    public async Task GetValidUserById_WhenUserDoesNotExist_ShouldAddNotification()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        await _userUseCase.GetValidUserById(userId);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Usuário" && n.Message == "O usuário não existe.");
    }

    [Fact]
    [Trait("Category", "User Application - Query")]
    public async Task SearchUsers_ShouldReturnPagedResponse()
    {
        // Arrange
        var request = new SearchUserRequest { Name = "test" };
        var person = new Person(PersonType.Physical, "FirstName", "LastName", "12345678901", DateTime.Now.AddYears(-20), "email@test.com");
        var user = new User(1, "Pass", "user.test", UserRole.Admin);
        
        // Usando reflexão para setar propriedades protegidas/navegação
        typeof(BaseEntity).GetProperty("Id")?.SetValue(user, 1);
        typeof(User).GetProperty("Person")?.SetValue(user, person);

        var users = new List<User> { user };
        var pagedResponse = new Shared.Response.PagedResponse<User>
        {
            Items = users,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _userRepositoryMock.Setup(x => x.SearchAsync(request)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _userUseCase.SearchUsers(request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}
