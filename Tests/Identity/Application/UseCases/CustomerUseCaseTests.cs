using Application.Identity.Interfaces;
using Application.Identity.UseCases;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using Domain.Shared.Abstractions;
using Moq;
using Shared.Identity.Request;
using Xunit;

namespace Tests.Identity.Application.UseCases;

public class CustomerUseCaseTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<IPersonUseCase> _personUseCaseMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly CustomerUseCase _useCase;

    public CustomerUseCaseTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _personUseCaseMock = new Mock<IPersonUseCase>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _useCase = new CustomerUseCase(
            _repositoryMock.Object,
            _personUseCaseMock.Object,
            _unitOfWorkMock.Object,
            _notificationContext
        );
    }

    [Fact]
    public async Task CreateCustomer_WithExistingPersonId_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateCustomerRequest { PersonId = 1, CreditLimit = 2000.00m };
        var person = new Person(PersonType.Physical, "Maria", "Silva", "98765432100", DateTime.UtcNow.AddYears(-25), "maria@example.com");

        typeof(BaseEntity).GetProperty("Id")?.SetValue(person, 1);
        _personUseCaseMock.Setup(p => p.GetPersonById(1)).ReturnsAsync(person);

        // Act
        var result = await _useCase.CreateCustomer(request);

        // Assert
        Assert.False(_notificationContext.IsInvalid);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_WithNonExistingPersonId_ShouldReturnNotification()
    {
        // Arrange
        var request = new CreateCustomerRequest { PersonId = 99 };
        _personUseCaseMock.Setup(p => p.GetPersonById(99)).ReturnsAsync((Person?)null);

        // Act
        var result = await _useCase.CreateCustomer(request);

        // Assert
        Assert.True(_notificationContext.IsInvalid);
        Assert.Contains(_notificationContext.Notifications, n => n.Key == "Person");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateCustomer_ExistingId_ShouldDeactivate()
    {
        // Arrange
        var customer = new Customer(personId: 1);
        _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(customer);

        // Act
        await _useCase.DeactivateCustomer(10);

        // Assert
        Assert.False(customer.IsActive);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
