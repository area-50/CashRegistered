using Application.Inventory.UseCases;
using Domain.Inventory.Entities;
using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Xunit;

namespace Tests.Application.Inventory.UseCases;

public class TagUseCaseTests
{
    private readonly Mock<ITagRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationContext _notificationContext;
    private readonly TagUseCase _useCase;

    public TagUseCaseTests()
    {
        _repositoryMock = new Mock<ITagRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        _useCase = new TagUseCase(_repositoryMock.Object, _unitOfWorkMock.Object, _notificationContext);
    }

    [Fact]
    public async Task CreateTag_ShouldCreateSuccessfully()
    {
        var request = new CreateTagRequest { Name = "Tag 1", ColorHex = "#FFFFFF" };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Tag>()))
            .Callback<Tag>(t => typeof(BaseEntity).GetProperty("Id")?.SetValue(t, 1));

        var result = await _useCase.CreateTag(request);

        result.Id.Should().NotBe(0);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Tag>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
