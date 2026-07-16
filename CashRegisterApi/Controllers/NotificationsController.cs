using Domain.Shared.Events;
using Domain.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IEventDispatcher _dispatcher;

    public NotificationsController(INotificationService notificationService, IEventDispatcher dispatcher)
    {
        _notificationService = notificationService;
        _dispatcher = dispatcher;
    }

    [HttpGet("stream/{topic}")]
    public async Task Stream(string topic)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var reader = await _notificationService.SubscribeAsync(topic, HttpContext.RequestAborted);
        
        await _dispatcher.Publish(new ClientConnectedToTopicEvent(topic));
 
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
