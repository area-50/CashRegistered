using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Security.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Identity.Domain.Entities;

public class UserTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public UserTests()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    [Fact]
    [Trait("Category", "User Domain - Validation")]
    public void Constructor_WithValidParameters_ShouldCreateUserAndBeValid()
    {
        // Arrange & Act
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);

        // Assert
        user.IsInvalid.Should().BeFalse();
        user.UserName.Should().Be("validUser");
        user.PersonId.Should().Be(1);
        user.UserRole.Should().Be(UserRole.Admin);
        user.IsActive.Should().BeTrue();
    }

    public static IEnumerable<object[]> InvalidConstructorData =>
        new List<object[]>
        {
            new object[] { "", "StrongPassword123", "Nome de usuário", "O nome de usuário é obrigatório." },
            new object[] { "usr", "StrongPassword123", "Nome de usuário", "O nome de usuário deve ter mais que 3 caracteres." },
            new object[] { "validUser", "", "Senha", "A senha é obrigatória." },
            new object[] { "validUser", "short", "Senha", "A senha deve ter pelo menos 12 caracteres." }
        };

    [Theory]
    [Trait("Category", "User Domain - Validation")]
    [MemberData(nameof(InvalidConstructorData))]
    public void Constructor_WithInvalidParameters_ShouldHaveNotifications(
        string userName, 
        string password, 
        string expectedKey, 
        string expectedMessage)
    {
        // Arrange & Act
        var user = new User(1, password, userName, UserRole.Logistics);

        // Assert
        user.IsInvalid.Should().BeTrue();
        user.Notifications.Should().Contain(n => n.Key == expectedKey && n.Message == expectedMessage);
    }

    [Fact]
    [Trait("Category", "User Domain - Validation")]
    public void ValidateUniqueUser_WhenUsernameExists_ShouldAddNotification()
    {
        // Arrange
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);

        // Act
        user.ValidateUniqueUser(userNameExists: true, personAlreadyHasUser: false);

        // Assert
        user.IsInvalid.Should().BeTrue();
        user.Notifications.Should().Contain(n => n.Key == "UserName" && n.Message == "Este nome de usuário já está em uso.");
    }

    [Fact]
    [Trait("Category", "User Domain - Validation")]
    public void ValidateUniqueUser_WhenPersonAlreadyHasUser_ShouldAddNotification()
    {
        // Arrange
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);

        // Act
        user.ValidateUniqueUser(userNameExists: false, personAlreadyHasUser: true);

        // Assert
        user.IsInvalid.Should().BeTrue();
        user.Notifications.Should().Contain(n => n.Key == "Person" && n.Message == "Esta pessoa já possui um usuário vinculado.");
    }

    [Fact]
    [Trait("Category", "User Domain - Password")]
    public void UpdatePassword_WithValidPassword_ShouldUpdateAndHash()
    {
        // Arrange
        var user = new User(1, "OldStrongPassword123", "validUser", UserRole.Admin);
        var newPassword = "NewStrongPassword123";
        var expectedHash = "new_hashed_value";
        _passwordHasherMock.Setup(x => x.HashPassword(newPassword)).Returns(expectedHash);

        // Act
        user.UpdatePassword(newPassword, _passwordHasherMock.Object);

        // Assert
        user.IsInvalid.Should().BeFalse();
        user.HashedPassword.Should().Be(expectedHash);
    }

    [Fact]
    [Trait("Category", "User Domain - Validation")]
    public void ValidateUserExists_WhenNull_ShouldAddNotificationToContext()
    {
        // Arrange
        User? user = null;
        var notificationContext = new global::Domain.Shared.Notifications.NotificationContext();

        // Act
        User.ValidateUserExists(user, notificationContext);

        // Assert
        notificationContext.IsInvalid.Should().BeTrue();
        notificationContext.Notifications.Should().Contain(n => n.Key == "Usuário" && n.Message == "O usuário não existe.");
    }

    [Fact]
    [Trait("Category", "User Domain - Password")]
    public void HashPassword_WithRawPassword_ShouldSetHashedPassword()
    {
        // Arrange
        var rawPassword = "StrongPassword123";
        var expectedHash = "hashed_value";
        _passwordHasherMock.Setup(x => x.HashPassword(rawPassword)).Returns(expectedHash);
        
        var user = new User(1, rawPassword, "validUser", UserRole.Admin);

        // Act
        user.HashPassword(_passwordHasherMock.Object);

        // Assert
        user.HashedPassword.Should().Be(expectedHash);
        _passwordHasherMock.Verify(x => x.HashPassword(rawPassword), Times.Once);
    }

    [Fact]
    [Trait("Category", "User Domain - Password")]
    public void AuthenticatePassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "StrongPassword123";
        var user = new User(1, password, "validUser", UserRole.Admin);
        user.HashedPassword = "correct_hash";

        _passwordHasherMock.Setup(x => x.VerifyHash(password, user.HashedPassword)).Returns(true);

        // Act
        var result = user.AuthenticatePassword(_passwordHasherMock.Object, password);

        // Assert
        result.Should().BeTrue();
        user.IsInvalid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "User Domain - Password")]
    public void AuthenticatePassword_WithWrongPassword_ShouldReturnFalseAndAddNotification()
    {
        // Arrange
        var password = "wrong_password";
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);
        user.HashedPassword = "correct_hash";

        _passwordHasherMock.Setup(x => x.VerifyHash(password, user.HashedPassword)).Returns(false);

        // Act
        var result = user.AuthenticatePassword(_passwordHasherMock.Object, password);

        // Assert
        result.Should().BeFalse();
        user.IsInvalid.Should().BeTrue();
        user.Notifications.Should().Contain(n => n.Key == "Login" && n.Message == "Usuário ou senha inválidos.");
    }

    [Fact]
    [Trait("Category", "User Domain - Status")]
    public void Deactivate_ShouldSetIsActiveToFalseAndSetUpdatedAt()
    {
        // Arrange
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "User Domain - Status")]
    public void Activate_ShouldSetIsActiveToTrueAndSetUpdatedAt()
    {
        // Arrange
        var user = new User(1, "StrongPassword123", "validUser", UserRole.Admin);
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
    }
}
