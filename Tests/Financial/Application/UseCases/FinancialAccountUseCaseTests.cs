using Application.Financial.UseCases;
using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Domain.Financial.Interfaces;
using FluentAssertions;
using Moq;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Domain.Shared.Notifications;
using Domain.Shared.Response;
using Xunit;

namespace Tests.Financial.Application.UseCases;

public class FinancialAccountUseCaseTests
{
    private readonly Mock<IFinancialAccountRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly FinancialAccountUseCase _useCase;

    public FinancialAccountUseCaseTests()
    {
        _repositoryMock = new Mock<IFinancialAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new FinancialAccountUseCase(_repositoryMock.Object, _notificationContext, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateFinancialAccount_ValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var request = new CreateFinancialAccountRequest
        {
            Name = "Conta Corrente Principal",
            AccountType = FinancialAccountType.CheckingAccount,
            ChartOfAccountsId = 10,
            BankCode = "341",
            BankName = "Itaú Unibanco",
            AgencyNumber = "1234",
            AccountNumber = "56789-0",
            Currency = "BRL",
            InitialBalance = 1500.00m
        };

        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FinancialAccount>()))
            .Callback<FinancialAccount>(a => typeof(BaseEntity).GetProperty("Id")?.SetValue(a, 1));

        // Act
        var result = await _useCase.CreateFinancialAccount(request);

        // Assert
        result.Id.Should().Be(1);
        _notificationContext.IsInvalid.Should().BeFalse();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FinancialAccount>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFinancialAccount_DuplicateName_ShouldReturnErrorNotification()
    {
        // Arrange
        var request = new CreateFinancialAccountRequest
        {
            Name = "Caixa Tesouraria",
            AccountType = FinancialAccountType.Cash,
            ChartOfAccountsId = 10
        };

        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(true);

        // Act
        var result = await _useCase.CreateFinancialAccount(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("Já existe"));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FinancialAccount>()), Times.Never);
    }

    [Fact]
    public async Task CreateFinancialAccount_InvalidData_ShouldAddNotifications()
    {
        // Arrange
        var request = new CreateFinancialAccountRequest
        {
            Name = "", // Inválido
            AccountType = FinancialAccountType.Cash,
            ChartOfAccountsId = 0 // Inválido
        };

        _repositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, null)).ReturnsAsync(false);

        // Act
        var result = await _useCase.CreateFinancialAccount(request);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome");
        _notificationContext.Notifications.Should().Contain(n => n.Key == "PlanoDeContas");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FinancialAccount>()), Times.Never);
    }

    [Fact]
    public async Task SearchFinancialAccounts_ShouldReturnPagedResponse()
    {
        // Arrange
        var request = new SearchFinancialAccountRequest { Page = 1, PageSize = 10 };
        var chartOfAccounts = new ChartOfAccounts("1.01.01", "Disponibilidades", AccountType.Asset, AccountNature.Debit);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(chartOfAccounts, 10);

        var account = new FinancialAccount("Caixa Central", FinancialAccountType.Cash, 10, initialBalance: 250.00m);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 5);
        typeof(FinancialAccount).GetProperty("ChartOfAccounts")?.SetValue(account, chartOfAccounts);

        var pagedResponse = new PagedResponse<FinancialAccount>
        {
            Items = new List<FinancialAccount> { account },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _repositoryMock.Setup(r => r.SearchAsync(request)).ReturnsAsync(pagedResponse);

        // Act
        var result = await _useCase.SearchFinancialAccounts(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        var item = result.Items.First();
        item.Id.Should().Be(5);
        item.Name.Should().Be("Caixa Central");
        item.AccountTypeName.Should().Be("Cash");
        item.ChartOfAccountsName.Should().Be("Disponibilidades");
        item.CurrentBalance.Should().Be(250.00m);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetFinancialAccountById_ExistingId_ShouldReturnResponse()
    {
        // Arrange
        var account = new FinancialAccount("Banco do Brasil", FinancialAccountType.CheckingAccount, 10);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 3);
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(account);

        // Act
        var result = await _useCase.GetFinancialAccountById(3);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.Name.Should().Be("Banco do Brasil");
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task GetFinancialAccountById_NonExistingId_ShouldAddNotification()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FinancialAccount?)null);

        // Act
        var result = await _useCase.GetFinancialAccountById(99);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "ContaBancaria" && n.Message.Contains("não encontrada"));
    }

    [Fact]
    public async Task UpdateFinancialAccount_ValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var account = new FinancialAccount("Nome Antigo", FinancialAccountType.Cash, 10);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.ExistsByNameAsync("Nome Novo", 2)).ReturnsAsync(false);

        var updateRequest = new UpdateFinancialAccountRequest
        {
            Name = "Nome Novo",
            AccountType = FinancialAccountType.CheckingAccount,
            ChartOfAccountsId = 10,
            Currency = "BRL",
            IsActive = true
        };

        // Act
        var result = await _useCase.UpdateFinancialAccount(2, updateRequest);

        // Assert
        result.Id.Should().Be(2);
        account.Name.Should().Be("Nome Novo");
        account.AccountType.Should().Be(FinancialAccountType.CheckingAccount);
        _repositoryMock.Verify(r => r.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFinancialAccount_DuplicateName_ShouldReturnErrorNotification()
    {
        // Arrange
        var account = new FinancialAccount("Minha Conta", FinancialAccountType.Cash, 10);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 2);
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.ExistsByNameAsync("Outra Conta", 2)).ReturnsAsync(true);

        var updateRequest = new UpdateFinancialAccountRequest
        {
            Name = "Outra Conta",
            AccountType = FinancialAccountType.Cash,
            ChartOfAccountsId = 10
        };

        // Act
        var result = await _useCase.UpdateFinancialAccount(2, updateRequest);

        // Assert
        result.Id.Should().Be(0);
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "Nome" && n.Message.Contains("Já existe"));
        _repositoryMock.Verify(r => r.Update(It.IsAny<FinancialAccount>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateFinancialAccount_ValidActiveAccount_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var account = new FinancialAccount("Conta Ativa", FinancialAccountType.Cash, 10);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 7);
        _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(account);

        // Act
        await _useCase.DeactivateFinancialAccount(7);

        // Assert
        account.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateFinancialAccount_AlreadyDeactivated_ShouldAddNotification()
    {
        // Arrange
        var account = new FinancialAccount("Conta Inativa", FinancialAccountType.Cash, 10);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(account, 7);
        account.Deactivate();
        _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(account);

        // Act
        await _useCase.DeactivateFinancialAccount(7);

        // Assert
        _notificationContext.IsInvalid.Should().BeTrue();
        _notificationContext.Notifications.Should().Contain(n => n.Key == "ContaBancaria" && n.Message.Contains("já está desativada"));
        _repositoryMock.Verify(r => r.Update(It.IsAny<FinancialAccount>()), Times.Never);
    }
}
