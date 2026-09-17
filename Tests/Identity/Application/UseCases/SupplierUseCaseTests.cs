using Application.Identity.Interfaces;
using Application.Identity.UseCases;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using Domain.Identity.ValueObjects;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Response;
using Moq;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Xunit;

namespace Tests.Identity.Application.UseCases;

public class SupplierUseCaseTests
{
    private readonly Mock<ISupplierRepository> _repositoryMock;
    private readonly Mock<IPersonUseCase> _personUseCaseMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly SupplierUseCase _useCase;

    public SupplierUseCaseTests()
    {
        _repositoryMock = new Mock<ISupplierRepository>();
        _personUseCaseMock = new Mock<IPersonUseCase>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _useCase = new SupplierUseCase(
            _repositoryMock.Object,
            _personUseCaseMock.Object,
            _unitOfWorkMock.Object,
            _notificationContext
        );
    }

    [Fact]
    public async Task CreateSupplier_WithExistingPersonId_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateSupplierRequest { PersonId = 1 };
        var person = new Person(PersonType.Physical, "John", "Doe", "12345678901", DateTime.UtcNow.AddYears(-20), "john@example.com");

        _personUseCaseMock.Setup(p => p.GetPersonById(1)).ReturnsAsync(person);

        // Act
        var result = await _useCase.CreateSupplier(request);

        // Assert
        Assert.False(_notificationContext.IsInvalid);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Supplier>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSupplier_WithNonExistingPersonId_ShouldReturnNotification()
    {
        // Arrange
        var request = new CreateSupplierRequest { PersonId = 99 };
        _personUseCaseMock.Setup(p => p.GetPersonById(99)).ReturnsAsync((Person?)null);

        // Act
        var result = await _useCase.CreateSupplier(request);

        // Assert
        Assert.True(_notificationContext.IsInvalid);
        Assert.Contains(_notificationContext.Notifications, n => n.Key == "Person");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Supplier>()), Times.Never);
    }

    [Fact]
    public async Task CreateSupplier_WithNewPerson_ShouldCreatePersonAndSupplier()
    {
        // Arrange
        var personRequest = new CreatePersonRequest
        {
            PersonType = "Physical",
            FirstName = "Jane",
            LastName = "Doe",
            TaxId = "98765432100",
            Birthdate = DateTime.UtcNow.AddYears(-25),
            Email = "jane@example.com"
        };
        var request = new CreateSupplierRequest { Person = personRequest };

        _personUseCaseMock.Setup(p => p.CreatePerson(personRequest)).ReturnsAsync(new CreateResponse { Id = 2 });

        // Act
        var result = await _useCase.CreateSupplier(request);

        // Assert
        Assert.False(_notificationContext.IsInvalid);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Supplier>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSupplierById_WhenExists_ShouldReturnSupplier()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "John", "Doe", "12345678901", DateTime.UtcNow.AddYears(-20), "john@example.com");
        var supplier = new Supplier(1) { Person = person };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        // Act
        var result = await _useCase.GetSupplierById(1);

        // Assert
        Assert.NotNull(result);
        Assert.False(_notificationContext.IsInvalid);
    }

    [Fact]
    public async Task GetSupplierById_WhenNotExists_ShouldReturnNotification()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Supplier?)null);

        // Act
        var result = await _useCase.GetSupplierById(1);

        // Assert
        Assert.True(_notificationContext.IsInvalid);
    }

    [Fact]
    public async Task SearchSuppliers_ShouldReturnPagedResults()
    {
        // Arrange
        var request = new SearchSupplierRequest { Page = 1, PageSize = 10 };
        var person = new Person(PersonType.Physical, "John", "Doe", "12345678901", DateTime.UtcNow.AddYears(-20), "john@example.com");
        var suppliers = new List<Supplier> { new Supplier(1) { Person = person } };
        var pagedResponse = new PagedResponse<Supplier> { Items = suppliers, TotalCount = 1, Page = 1, PageSize = 10 };

        _repositoryMock.Setup(r => r.SearchAsync(request)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _useCase.SearchSuppliers(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task UpdateSupplier_WhenExists_ShouldUpdateAndCommit()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "John", "Doe", "12345678901", DateTime.UtcNow.AddYears(-20), "john@example.com");
        var supplier = new Supplier(1) { Person = person };
        var request = new UpdateSupplierRequest { IsActive = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        // Act
        await _useCase.UpdateSupplier(1, request);

        // Assert
        Assert.False(_notificationContext.IsInvalid);
        _repositoryMock.Verify(r => r.Update(supplier), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateSupplier_WhenExists_ShouldDeactivateAndCommit()
    {
        // Arrange
        var person = new Person(PersonType.Physical, "John", "Doe", "12345678901", DateTime.UtcNow.AddYears(-20), "john@example.com");
        var supplier = new Supplier(1) { Person = person };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

        // Act
        await _useCase.DeactivateSupplier(1);

        // Assert
        Assert.False(_notificationContext.IsInvalid);
        Assert.False(supplier.IsActive);
        _repositoryMock.Verify(r => r.Update(supplier), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
