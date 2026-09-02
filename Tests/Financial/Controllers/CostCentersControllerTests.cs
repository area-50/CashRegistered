using Application.Financial.Interfaces;
using CashRegister.Controllers.Financial;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Domain.Shared.DTOs;
using Shared.Financial.Request;
using Shared.Financial.Response;
using Domain.Shared.Response;
using Xunit;

namespace Tests.Financial.Controllers;

public class CostCentersControllerTests
{
    private readonly Mock<ICostCenterUseCase> _useCaseMock;
    private readonly CostCentersController _controller;

    public CostCentersControllerTests()
    {
        _useCaseMock = new Mock<ICostCenterUseCase>();
        _controller = new CostCentersController(_useCaseMock.Object);
    }

    [Fact]
    public async Task CreateCostCenter_ShouldReturnOk()
    {
        // Arrange
        var request = new CreateCostCenterRequest { Name = "TI", ManagerId = 1 };
        var expectedResponse = new CreateResponse { Id = 1 };
        _useCaseMock.Setup(u => u.CreateCostCenter(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateCostCenter(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SearchCostCenters_ShouldReturnOk()
    {
        // Arrange
        var request = new SearchCostCenterRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<GetSearchCostCenterResponse>
        {
            Items = new List<GetSearchCostCenterResponse>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };
        _useCaseMock.Setup(u => u.SearchCostCenters(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchCostCenters(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetCostCenterById_ShouldReturnOk()
    {
        // Arrange
        var expectedResponse = new GetCostCenterByIdResponse { Id = 5, Name = "TI", ManagerId = 1, ManagerName = "João Silva", IsActive = true };
        _useCaseMock.Setup(u => u.GetCostCenterById(5)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCostCenterById(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UpdateCostCenter_ShouldReturnOk()
    {
        // Arrange
        var request = new UpdateCostCenterRequest { Name = "TI Refatorado", ManagerId = 2, IsActive = true };
        var expectedResponse = new UpdateResponse { Id = 5 };
        _useCaseMock.Setup(u => u.UpdateCostCenter(5, request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateCostCenter(5, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task DeactivateCostCenter_ShouldReturnOk()
    {
        // Act
        var result = await _controller.DeactivateCostCenter(5);

        // Assert
        result.Should().BeOfType<OkResult>();
        _useCaseMock.Verify(u => u.DeactivateCostCenter(5), Times.Once);
    }
}
