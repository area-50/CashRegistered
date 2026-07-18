using Application.Identity.UseCases;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Identity.Request;
using Shared.Notifications;

namespace Tests.Application.Identity.UseCases;

public class PersonUseCaseTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly PersonUseCase _personUseCase;

    public PersonUseCaseTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _personUseCase = new PersonUseCase(
            _personRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _notificationContext
        );
    }

    [Fact]
    [Trait("Category", "Person Application - Create")]
    public async Task CreatePerson_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var request = new CreatePersonRequest
        {
            FirstName = "Fernando",
            LastName = "Mendes",
            TaxId = "12345678901",
            Birthdate = DateTime.Now.AddYears(-30),
            Email = "fernando@test.com",
            PersonType = "Physical"
        };

        // Act
        var response = await _personUseCase.CreatePerson(request);

        // Assert
        _notificationContext.IsInvalid.Should().BeFalse();
        _personRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Person>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    [Trait("Category", "Person Application - Create")]
    public async Task CreatePerson_WithInvalidData_ShouldAddNotifications()
    {
        // Arrange
        var request = new CreatePersonRequest
        {
            FirstName = "Fernando",
            LastName = "Mendes",
            TaxId = "", // Invalid
            Birthdate = DateTime.Now,
            Email = "invalid-email", // Invalid
            PersonType = "Physical"
        };

        // Act
        var response = await _personUseCase.CreatePerson(request);

        // Assert
        response.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    [Trait("Category", "Person Application - Query")]
    public async Task GetAllPeople_ShouldReturnList()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person(PersonType.Physical, "F1", "L1", "123", DateTime.Now, "e1@t.com"),
            new Person(PersonType.Physical, "F2", "L2", "456", DateTime.Now, "e2@t.com")
        };
        _personRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Person, bool>>>()))
            .ReturnsAsync(people);

        // Act
        var result = await _personUseCase.GetAllPeople();

        // Assert
        result.Should().HaveCount(2);
    }
}
