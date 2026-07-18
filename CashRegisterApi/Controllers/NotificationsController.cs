using Domain.Shared.Events;
using Domain.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashRegister.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(INotificationService notificationService, IEventDispatcher dispatcher)
    : ControllerBase
{
    [HttpGet("stream/inventory.requisitions.pending")]
    [Authorize(Policy = "LogisticsOnly")]
    public async Task StreamInventoryRequisitionsPending()
    {
        await HandleStreamConnection(Domain.Shared.Constants.NotificationTopics.InventoryRequisitionsPending);
    }

    private async Task HandleStreamConnection(string topic)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var reader = await notificationService.SubscribeAsync(topic, HttpContext.RequestAborted);
        
        await dispatcher.Publish(new ClientConnectedToTopicEvent(topic));
 
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
