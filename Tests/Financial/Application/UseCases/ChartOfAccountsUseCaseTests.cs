using Application.Financial.UseCases;
using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Domain.Financial.Interfaces;
using Domain.Shared.Abstractions;
using Domain.Shared.Notifications;
using Domain.Shared.Response;
using FluentAssertions;
using Moq;
using Domain.Shared.DTOs;
using Shared.Financial.Request;
using Xunit;


namespace Tests.Financial.Application.UseCases;

public class ChartOfAccountsUseCaseTests
{
    private readonly Mock<IChartOfAccountsRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly ChartOfAccountsUseCase _useCase;

    public ChartOfAccountsUseCaseTests()
    {
        _repositoryMock = new Mock<IChartOfAccountsRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new ChartOfAccountsUseCase(_repositoryMock.Object, _notificationContext, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateChartOfAccounts_ValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest
        {
            Code = "1.1.01.001",
            Name = "Caixa Geral",
            AccountType = AccountType.Asset,
            Nature = AccountNature.Debit,
            IsSynthetic = false,
            AllowPosting = true
        };

        _repositoryMock.Setup(r => r.ExistsByCodeAsync(request.Code, null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ChartOfAccounts>()))
            .Callback<ChartOfAccounts>(a => typeof(BaseEntity).GetProperty("Id")?.SetValue(a, 1));

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(1);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<ChartOfAccounts>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateChartOfAccounts_WithValidSyntheticParent_ShouldCreateSuccessfully()
    {
        // Arrange
        var parentAccount = new ChartOfAccounts("1.1", "Ativo Circulante", AccountType.Asset, AccountNature.Debit, null, isSynthetic: true);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(parentAccount, 10);

        var request = new CreateChartOfAccountsRequest
        {
            Code = "1.1.01",
            Name = "Disponibilidades",
            AccountType = AccountType.Asset,
            Nature = AccountNature.Debit,
            ParentAccountId = 10,
            IsSynthetic = false,
            AllowPosting = true
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(parentAccount);
        _repositoryMock.Setup(r => r.ExistsByCodeAsync(request.Code, null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ChartOfAccounts>()))
            .Callback<ChartOfAccounts>(a => typeof(BaseEntity).GetProperty("Id")?.SetValue(a, 2));

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(2);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateChartOfAccounts_ParentNotFound_ShouldReturnErrorNotification()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest
        {
            Code = "1.1.01",
            Name = "Disponibilidades",
            AccountType = AccountType.Asset,
            Nature = AccountNature.Debit,
            ParentAccountId = 99
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ChartOfAccounts?)null);

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "ContaPai" && n.Message == "Conta pai informada não existe.");
    }

    [Fact]
    public async Task CreateChartOfAccounts_ParentNotSynthetic_ShouldReturnErrorNotification()
    {
        // Arrange
        var parentAccount = new ChartOfAccounts("1.1.01", "Disponibilidade Analítica", AccountType.Asset, AccountNature.Debit, null, isSynthetic: false);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(parentAccount, 10);

        var request = new CreateChartOfAccountsRequest
        {
            Code = "1.1.01.001",
            Name = "Caixa",
            AccountType = AccountType.Asset,
            Nature = AccountNature.Debit,
            ParentAccountId = 10
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(parentAccount);

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "ContaPai" && n.Message == "Apenas contas sintéticas podem ser selecionadas como conta pai.");
    }

    [Fact]
    public async Task CreateChartOfAccounts_DuplicateCode_ShouldReturnErrorNotification()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest { Code = "1.1.01", Name = "Conta Duplicada" };
        _repositoryMock.Setup(r => r.ExistsByCodeAsync("1.1.01", null)).ReturnsAsync(true);

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "Codigo" && n.Message == "Já existe um plano de contas cadastrado com este código.");
    }

    [Fact]
    public async Task CreateChartOfAccounts_DuplicateName_ShouldReturnErrorNotification()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest { Code = "1.1.02", Name = "Caixa Geral" };
        _repositoryMock.Setup(r => r.ExistsByCodeAsync("1.1.02", null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsByNameAsync("Caixa Geral", null)).ReturnsAsync(true);

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "Nome" && n.Message == "Já existe um plano de contas cadastrado com este nome.");
    }

    [Fact]
    public async Task CreateChartOfAccounts_InvalidEntity_ShouldReturnErrorNotification()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest { Code = "", Name = "" };

        // Act
        var result = await _useCase.CreateChartOfAccounts(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
    }

    [Fact]
    public async Task SearchChartOfAccounts_ShouldReturnMappedPagedResult()
    {
        // Arrange
        var request = new SearchChartOfAccountsRequest { Page = 1, PageSize = 10 };
        var accounts = new List<ChartOfAccounts>
        {
            new ChartOfAccounts("1.1", "Ativo Circulante", AccountType.Asset, AccountNature.Debit, null, true)
        };
        var pagedResponse = new PagedResponse<ChartOfAccounts>
        {
            Items = accounts,
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _repositoryMock.Setup(r => r.SearchAsync(request)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _useCase.SearchChartOfAccounts(request);

        // Assert
        result.TotalCount.Should().Be(1);

        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("1.1");
    }

    [Fact]
    public async Task GetChartOfAccountsById_ExistingAccount_ShouldReturnData()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1", "Ativo Circulante", AccountType.Asset, AccountNature.Debit, null, true);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        // Act
        var result = await _useCase.GetChartOfAccountsById(5);

        // Assert
        result.Id.Should().Be(5);
        result.Code.Should().Be("1.1");
        result.Name.Should().Be("Ativo Circulante");
    }

    [Fact]
    public async Task GetChartOfAccountsById_NotFound_ShouldReturnNotification()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ChartOfAccounts?)null);

        // Act
        var result = await _useCase.GetChartOfAccountsById(99);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "PlanoDeContas" && n.Message == "Plano de contas não encontrado.");
    }

    [Fact]
    public async Task UpdateChartOfAccounts_ValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01", "Disponibilidades Old", AccountType.Asset, AccountNature.Debit);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);

        var updateRequest = new UpdateChartOfAccountsRequest
        {
            Code = "1.1.01",
            Name = "Disponibilidades Atualizado",
            AccountType = AccountType.Asset,
            Nature = AccountNature.Debit,
            IsSynthetic = false,
            AllowPosting = true
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.ExistsByCodeAsync(updateRequest.Code, 2)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(updateRequest.Name, 2)).ReturnsAsync(false);

        // Act
        var result = await _useCase.UpdateChartOfAccounts(2, updateRequest);

        // Assert
        result.Id.Should().Be(2);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateChartOfAccounts_SelfParent_ShouldReturnErrorNotification()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01", "Disponibilidades", AccountType.Asset, AccountNature.Debit);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);

        var updateRequest = new UpdateChartOfAccountsRequest
        {
            Code = "1.1.01",
            Name = "Disponibilidades",
            ParentAccountId = 5
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        // Act
        var result = await _useCase.UpdateChartOfAccounts(5, updateRequest);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "ContaPai" && n.Message == "Uma conta não pode ser pai dela mesma.");
    }

    [Fact]
    public async Task UpdateChartOfAccounts_NotFound_ShouldReturnErrorNotification()
    {
        // Arrange
        var updateRequest = new UpdateChartOfAccountsRequest { Code = "1.1", Name = "Test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ChartOfAccounts?)null);

        // Act
        var result = await _useCase.UpdateChartOfAccounts(99, updateRequest);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "PlanoDeContas" && n.Message == "Plano de contas não encontrado.");
    }

    [Fact]
    public async Task DeactivateChartOfAccounts_ExistingAccount_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var account = new ChartOfAccounts("1.1.01", "Disponibilidades", AccountType.Asset, AccountNature.Debit);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(account);

        // Act
        await _useCase.DeactivateChartOfAccounts(5);

        // Assert
        account.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateChartOfAccounts_NotFound_ShouldReturnNotification()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ChartOfAccounts?)null);

        // Act
        await _useCase.DeactivateChartOfAccounts(99);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().ContainSingle(n => n.Key == "PlanoDeContas" && n.Message == "Plano de contas não encontrado.");
    }
}
