using Application.Decorators;
using Application.Interfaces;
using Application.Services;
using Application.Services.Strategies;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Abstractions;
using Shared.Notifications;

namespace Tests.Application.Decorators;

public class UnitOfWorkDecoratorTests
{
    private readonly Mock<IUnitOfWork> _innerMock;
    private readonly NotificationContext _notificationContext;
    private readonly PersistenceExceptionHandler _exceptionHandler;

    public UnitOfWorkDecoratorTests()
    {
        _innerMock = new Mock<IUnitOfWork>();
        _notificationContext = new NotificationContext();
        var loggerMock = new Mock<ILogger<PersistenceExceptionHandler>>();
        var infraStrategy = new InfrastructureFailureStrategy(new Mock<ILogger<InfrastructureFailureStrategy>>().Object);
        var strategies = new List<IPersistenceExceptionStrategy> { infraStrategy };
        _exceptionHandler = new PersistenceExceptionHandler(strategies, _notificationContext, loggerMock.Object);
    }

    [Fact]
    public async Task CommitAsync_ShouldReturnTrue_WhenNoExceptionOccurs()
    {
        _innerMock.Setup(x => x.CommitAsync()).ReturnsAsync(true);
        var decorator = new UnitOfWorkDecorator(_innerMock.Object, _exceptionHandler);

        var result = await decorator.CommitAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CommitAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        _innerMock.Setup(x => x.CommitAsync()).ThrowsAsync(new Exception("Database error"));
        var decorator = new UnitOfWorkDecorator(_innerMock.Object, _exceptionHandler);

        var result = await decorator.CommitAsync();

        result.Should().BeFalse();
        _notificationContext.IsInvalid.Should().BeTrue();
    }
}
