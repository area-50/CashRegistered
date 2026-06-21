using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;

namespace Tests.Application.Inventory.UseCases;

public class CategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly CategoryUseCase _useCase;

    public CategoryUseCaseTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new CategoryUseCase(_repositoryMock.Object, _unitOfWorkMock.Object, _notificationContext);
    }

    [Fact]
    public async Task CreateCategory_ShouldCreateSuccessfully()
    {
        var request = new CreateCategoryRequest { Name = "Category 1" };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Category>()))
            .Callback<Category>(c => typeof(BaseEntity).GetProperty("Id")?.SetValue(c, 1));

        var result = await _useCase.CreateCategory(request);

        result.Id.Should().NotBe(0);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
