using Application.Financial.Interfaces;
using CashRegister.Controllers.Financial;
using Domain.Shared.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Domain.Shared.DTOs;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Xunit;


namespace Tests.Financial.Controllers;

public class ChartOfAccountsControllerTests
{
    private readonly Mock<IChartOfAccountsUseCase> _useCaseMock;
    private readonly ChartOfAccountsController _controller;

    public ChartOfAccountsControllerTests()
    {
        _useCaseMock = new Mock<IChartOfAccountsUseCase>();
        _controller = new ChartOfAccountsController(_useCaseMock.Object);
    }

    [Fact]
    public async Task CreateChartOfAccounts_ShouldReturnOk()
    {
        // Arrange
        var request = new CreateChartOfAccountsRequest { Code = "1.1.01", Name = "Conta Teste" };
        var expectedResponse = new CreateResponse { Id = 1 };
        _useCaseMock.Setup(u => u.CreateChartOfAccounts(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateChartOfAccounts(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SearchChartOfAccounts_ShouldReturnOk()
    {
        // Arrange
        var request = new SearchChartOfAccountsRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<GetSearchChartOfAccountsResponse>
        {
            Items = new List<GetSearchChartOfAccountsResponse>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0

        };
        _useCaseMock.Setup(u => u.SearchChartOfAccounts(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchChartOfAccounts(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetChartOfAccountsById_ShouldReturnOk()
    {
        // Arrange
        var expectedResponse = new GetChartOfAccountsByIdResponse { Id = 5, Code = "1.1", Name = "Conta 5" };
        _useCaseMock.Setup(u => u.GetChartOfAccountsById(5)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetChartOfAccountsById(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UpdateChartOfAccounts_ShouldReturnOk()
    {
        // Arrange
        var request = new UpdateChartOfAccountsRequest { Code = "1.1.01", Name = "Conta Atualizada" };
        var expectedResponse = new UpdateResponse { Id = 5 };
        _useCaseMock.Setup(u => u.UpdateChartOfAccounts(5, request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateChartOfAccounts(5, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task DeactivateChartOfAccounts_ShouldReturnOk()
    {
        // Act
        var result = await _controller.DeactivateChartOfAccounts(5);

        // Assert
        result.Should().BeOfType<OkResult>();
        _useCaseMock.Verify(u => u.DeactivateChartOfAccounts(5), Times.Once);
    }
}
