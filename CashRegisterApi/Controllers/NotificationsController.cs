using Domain.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Domain.Shared.Events;

namespace CashRegisterApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public NotificationsController(INotificationService notificationService, IMediator mediator)
    {
        _notificationService = notificationService;
        _mediator = mediator;
    }

    [HttpGet("stream/{topic}")]
    public async Task Stream(string topic)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var reader = await _notificationService.SubscribeAsync(topic, HttpContext.RequestAborted);

        // Avisa o ecossistema que alguém quer dados desse tópico
        await _mediator.Publish(new ClientConnectedToTopicEvent(topic));

        try
        {
            await foreach (var message in reader.ReadAllAsync(HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"data: {message}\n\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when client disconnects
        }
    }
}
