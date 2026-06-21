using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Notifications;

namespace CashRegister.Middlewares;

public class NotificationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, NotificationContext notificationContext)
    {
        await next(context);

        if (notificationContext.IsInvalid)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var notifications = notificationContext.Notifications.Select(n => new 
            {
                Property = n.Key,
                Message = n.Message
            });

            var responseJson = JsonSerializer.Serialize(new Shared.Response.ApiResponse<object>
            {
                Success = false,
                Errors = notifications
            });
            await context.Response.WriteAsync(responseJson);
        }
    }
}
