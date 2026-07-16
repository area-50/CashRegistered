using Application.Identity.Interfaces;
using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Shared.Inventory.Request;
using Shared.Notifications;
using Shared.ValueObjects;

namespace Tests.Application.Inventory.UseCases;

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
        _useCase = new SupplierUseCase(_repositoryMock.Object, _personUseCaseMock.Object, _unitOfWorkMock.Object, _notificationContext);
    }

    [Fact]
    public async Task CreateSupplier_WithExistingPerson_ShouldCreateSuccessfully()
    {
        var request = new CreateSupplierRequest { PersonId = 1 };
        
        var existingPerson = new global::Domain.Identity.Entities.Person(global::Domain.Identity.Enums.PersonType.Legal, "Test", "Person", "12345678901234", System.DateTime.Now, "test@test.com");
        typeof(BaseEntity).GetProperty("Id")?.SetValue(existingPerson, 1);

        _personUseCaseMock.Setup(x => x.GetPersonById(1))
            .ReturnsAsync(existingPerson);

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Supplier>()))
            .Callback<Supplier>(c => typeof(BaseEntity).GetProperty("Id")?.SetValue(c, 1));

        var result = await _useCase.CreateSupplier(request);

        result.Id.Should().Be(1);
        _personUseCaseMock.Verify(x => x.GetPersonById(1), Times.Once);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Supplier>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSupplier_WithNewPerson_ShouldCreateSuccessfully()
    {
        var request = new CreateSupplierRequest 
        { 
            PersonId = 0,
            Person = new CreatePersonRequest { FirstName = "New", LastName = "Supplier" }
        };

        _personUseCaseMock.Setup(x => x.CreatePerson(It.IsAny<CreatePersonRequest>()))
            .ReturnsAsync(new Shared.Response.CreateResponse { Id = 2 });

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Supplier>()))
            .Callback<Supplier>(c => typeof(BaseEntity).GetProperty("Id")?.SetValue(c, 1));

        var result = await _useCase.CreateSupplier(request);

        result.Id.Should().Be(1);
        _personUseCaseMock.Verify(x => x.CreatePerson(It.IsAny<CreatePersonRequest>()), Times.Once);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Supplier>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task GetSupplierById_ShouldReturnSupplier_WhenExists()
    {
        var supplier = new Supplier(1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(supplier, 1);
        
        var person = new global::Domain.Identity.Entities.Person(global::Domain.Identity.Enums.PersonType.Legal, "Test", "Person", "12345678901234", System.DateTime.Now, "test@test.com");
        typeof(Supplier).GetProperty("Person")?.SetValue(supplier, person);

        _repositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(supplier);

        var result = await _useCase.GetSupplierById(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.PersonId.Should().Be(1);
        result.Name.Should().NotBeNull();
    }
}
