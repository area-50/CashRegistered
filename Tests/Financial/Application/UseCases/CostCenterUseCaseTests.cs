using Application.Financial.UseCases;
using Application.Identity.Interfaces;
using Domain.Financial.Entities;
using Domain.Financial.Interfaces;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using FluentAssertions;
using Moq;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Domain.Shared.Notifications;
using Domain.Shared.Response;
using Xunit;

namespace Tests.Financial.Application.UseCases;

public class CostCenterUseCaseTests
{
    private readonly Mock<ICostCenterRepository> _repositoryMock;
    private readonly Mock<IUserUseCase> _userUseCaseMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly CostCenterUseCase _useCase;

    public CostCenterUseCaseTests()
    {
        _repositoryMock = new Mock<ICostCenterRepository>();
        _userUseCaseMock = new Mock<IUserUseCase>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();

        _useCase = new CostCenterUseCase(
            _repositoryMock.Object,
            _userUseCaseMock.Object,
            _unitOfWorkMock.Object,
            _notificationContext
        );
    }

    [Fact]
    public async Task CreateCostCenter_ValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var request = new CreateCostCenterRequest { Name = "TI", ManagerId = 1 };
        var managerUser = new User(1, "Password12345!", "manager", UserRole.Admin);

        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _userUseCaseMock.Setup(u => u.GetUserById(request.ManagerId)).ReturnsAsync(managerUser);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CostCenter>()))
            .Callback<CostCenter>(c => typeof(BaseEntity).GetProperty("Id")?.SetValue(c, 10));

        // Act
        var result = await _useCase.CreateCostCenter(request);

        // Assert
        result.Id.Should().Be(10);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CostCenter>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCostCenter_DuplicateName_ShouldAddNotificationAndReturnZeroId()
    {
        // Arrange
        var request = new CreateCostCenterRequest { Name = "TI", ManagerId = 1 };
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(true);

        // Act
        var result = await _useCase.CreateCostCenter(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("Já existe"));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CostCenter>()), Times.Never);
    }

    [Fact]
    public async Task CreateCostCenter_ManagerNotFound_ShouldAddNotificationAndReturnZeroId()
    {
        // Arrange
        var request = new CreateCostCenterRequest { Name = "TI", ManagerId = 99 };
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _userUseCaseMock.Setup(u => u.GetUserById(request.ManagerId)).ReturnsAsync((User?)null);

        // Act
        var result = await _useCase.CreateCostCenter(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "GerenteId" && n.Message.Contains("não foi encontrado"));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CostCenter>()), Times.Never);
    }

    [Fact]
    public async Task CreateCostCenter_InvalidEntity_ShouldAddNotificationAndReturnZeroId()
    {
        // Arrange
        var request = new CreateCostCenterRequest { Name = "", ManagerId = 1 };
        var managerUser = new User(1, "Password12345!", "manager", UserRole.Admin);

        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _userUseCaseMock.Setup(u => u.GetUserById(request.ManagerId)).ReturnsAsync(managerUser);

        // Act
        var result = await _useCase.CreateCostCenter(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome");
    }

    [Fact]
    public async Task UpdateCostCenter_ValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var costCenter = new CostCenter("TI Antigo", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 5);

        var request = new UpdateCostCenterRequest { Name = "TI Novo", ManagerId = 2, IsActive = true };
        var managerUser = new User(2, "Password12345!", "manager2", UserRole.Admin);

        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(costCenter);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, 5)).ReturnsAsync(false);
        _userUseCaseMock.Setup(u => u.GetUserById(request.ManagerId)).ReturnsAsync(managerUser);

        // Act
        var result = await _useCase.UpdateCostCenter(5, request);

        // Assert
        result.Id.Should().Be(5);
        costCenter.Name.Should().Be("TI Novo");
        costCenter.ManagerId.Should().Be(2);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(costCenter), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCostCenter_NotFound_ShouldAddNotificationAndReturnZeroId()
    {
        // Arrange
        var request = new UpdateCostCenterRequest { Name = "TI Novo", ManagerId = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((CostCenter?)null);

        // Act
        var result = await _useCase.UpdateCostCenter(5, request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "CentroDeCusto" && n.Message.Contains("não existe"));
    }

    [Fact]
    public async Task UpdateCostCenter_DuplicateName_ShouldAddNotificationAndReturnZeroId()
    {
        // Arrange
        var costCenter = new CostCenter("TI Antigo", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 5);

        var request = new UpdateCostCenterRequest { Name = "Financeiro", ManagerId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(costCenter);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, 5)).ReturnsAsync(true);

        // Act
        var result = await _useCase.UpdateCostCenter(5, request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("Já existe"));
    }

    [Fact]
    public async Task UpdateCostCenter_DeactivateToggle_ShouldDeactivateEntity()
    {
        // Arrange
        var costCenter = new CostCenter("Operações", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 3);

        var request = new UpdateCostCenterRequest { Name = "Operações", ManagerId = 1, IsActive = false };
        var managerUser = new User(1, "Password12345!", "manager", UserRole.Admin);

        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(costCenter);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, 3)).ReturnsAsync(false);
        _userUseCaseMock.Setup(u => u.GetUserById(request.ManagerId)).ReturnsAsync(managerUser);

        // Act
        var result = await _useCase.UpdateCostCenter(3, request);

        // Assert
        result.Id.Should().Be(3);
        costCenter.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateCostCenter_ActiveEntity_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var costCenter = new CostCenter("Logística", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 2);

        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(costCenter);

        // Act
        await _useCase.DeactivateCostCenter(2);

        // Assert
        costCenter.IsActive.Should().BeFalse();
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(costCenter), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateCostCenter_AlreadyInactive_ShouldAddNotification()
    {
        // Arrange
        var costCenter = new CostCenter("Logística", 1);
        costCenter.Deactivate();
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 2);

        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(costCenter);

        // Act
        await _useCase.DeactivateCostCenter(2);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "CentroDeCusto" && n.Message.Contains("já está desativado"));
    }

    [Fact]
    public async Task GetCostCenterById_Existing_ShouldReturnMappedResponse()
    {
        // Arrange
        var costCenter = new CostCenter("Recursos Humanos", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter, 7);

        var person = new Person(PersonType.Physical, "Carlos", "Silva", "12345678901", DateTime.Now, "carlos@test.com");
        var managerUser = new User(1, "Password12345!", "carlos", UserRole.Admin);
        typeof(User).GetProperty("Person")?.SetValue(managerUser, person);
        typeof(CostCenter).GetProperty("Manager")?.SetValue(costCenter, managerUser);

        _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(costCenter);

        // Act
        var result = await _useCase.GetCostCenterById(7);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
        result.Name.Should().Be("Recursos Humanos");
        result.ManagerName.Should().Be("Carlos Silva");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCostCenterById_NotFound_ShouldAddNotificationAndReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CostCenter?)null);

        // Act
        var result = await _useCase.GetCostCenterById(99);

        // Assert
        result.Should().BeNull();
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "CentroDeCusto");
    }

    [Fact]
    public async Task SearchCostCenters_ShouldReturnPagedResponse()
    {
        // Arrange
        var costCenter1 = new CostCenter("TI", 1);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter1, 1);
        var person1 = new Person(PersonType.Physical, "Ana", "Souza", "11111111111", DateTime.Now, "ana@test.com");
        var user1 = new User(1, "Password12345!", "ana", UserRole.Admin);
        typeof(User).GetProperty("Person")?.SetValue(user1, person1);
        typeof(CostCenter).GetProperty("Manager")?.SetValue(costCenter1, user1);

        var costCenter2 = new CostCenter("Vendas", 2);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(costCenter2, 2);

        var pagedList = new PagedResponse<CostCenter>
        {
            Items = new List<CostCenter> { costCenter1, costCenter2 },
            Page = 1,
            PageSize = 10,
            TotalCount = 2
        };

        var request = new SearchCostCenterRequest { Page = 1, PageSize = 10 };
        _repositoryMock.Setup(r => r.SearchAsync(request)).ReturnsAsync(pagedList);

        // Act
        var result = await _useCase.SearchCostCenters(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.First().ManagerName.Should().Be("Ana Souza");
        result.Items.Last().ManagerName.Should().Be(string.Empty);
    }
}
