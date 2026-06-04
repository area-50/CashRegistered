using CashRegisterApi.Controllers.Identity;
using Application.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Shared.Response;
using FluentAssertions;

namespace Tests.CashRegisterApi.Controllers.Identity;

public class PersonControllerTests
{
    private readonly Mock<IPersonUseCase> _personUseCaseMock;
    private readonly PersonController _controller;

    public PersonControllerTests()
    {
        _personUseCaseMock = new Mock<IPersonUseCase>();
        _controller = new PersonController(_personUseCaseMock.Object);
    }

    [Fact]
    [Trait("Category", "Person Controller - Create")]
    public async Task CreatePerson_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreatePersonRequest { FirstName = "Test" };
        var response = new CreateResponse { Id = 1 };
        _personUseCaseMock.Setup(x => x.CreatePerson(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreatePerson(request);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        _personUseCaseMock.Verify(x => x.CreatePerson(request), Times.Once);
    }

    [Fact]
    [Trait("Category", "Person Controller - Query")]
    public async Task GetPeople_ShouldReturnOk()
    {
        // Arrange
        var response = new List<GetAllPeopleResponse>();
        _personUseCaseMock.Setup(x => x.GetAllPeople()).ReturnsAsync(response);

        // Act
        var result = await _controller.GetPeople();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
