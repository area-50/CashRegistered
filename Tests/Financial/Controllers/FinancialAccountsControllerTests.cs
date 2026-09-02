using Application.Financial.Interfaces;
using CashRegister.Controllers.Financial;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Domain.Shared.DTOs;
using Shared.Financial.Response;
using Domain.Shared.Response;
using Xunit;

namespace Tests.Financial.Controllers;

public class FinancialAccountsControllerTests
{
    private readonly Mock<IFinancialAccountUseCase> _useCaseMock;
    private readonly FinancialAccountsController _controller;

    public FinancialAccountsControllerTests()
    {
        _useCaseMock = new Mock<IFinancialAccountUseCase>();
        _controller = new FinancialAccountsController(_useCaseMock.Object);
    }

    [Fact]
    public async Task CreateFinancialAccount_ShouldReturnOk()
    {
        // Arrange
        var request = new CreateFinancialAccountRequest { Name = "Conta Teste" };
        var expectedResponse = new CreateResponse { Id = 1 };
        _useCaseMock.Setup(u => u.CreateFinancialAccount(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateFinancialAccount(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SearchFinancialAccounts_ShouldReturnOk()
    {
        // Arrange
        var request = new SearchFinancialAccountRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<GetSearchFinancialAccountResponse>
        {
            Items = new List<GetSearchFinancialAccountResponse>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };
        _useCaseMock.Setup(u => u.SearchFinancialAccounts(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchFinancialAccounts(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetFinancialAccountById_ShouldReturnOk()
    {
        // Arrange
        var expectedResponse = new GetFinancialAccountByIdResponse { Id = 5, Name = "Conta 5" };
        _useCaseMock.Setup(u => u.GetFinancialAccountById(5)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetFinancialAccountById(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UpdateFinancialAccount_ShouldReturnOk()
    {
        // Arrange
        var request = new UpdateFinancialAccountRequest { Name = "Conta Atualizada" };
        var expectedResponse = new UpdateResponse { Id = 5 };
        _useCaseMock.Setup(u => u.UpdateFinancialAccount(5, request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateFinancialAccount(5, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task DeactivateFinancialAccount_ShouldReturnOk()
    {
        // Act
        var result = await _controller.DeactivateFinancialAccount(5);

        // Assert
        result.Should().BeOfType<OkResult>();
        _useCaseMock.Verify(u => u.DeactivateFinancialAccount(5), Times.Once);
    }
}
