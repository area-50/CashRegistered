using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Notifications;

namespace CashRegister.Filters;

public class NotificationFilter(NotificationContext notificationContext) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();
        
        if (notificationContext.IsInvalid)
        {
            var notifications = notificationContext.Notifications.Select(n => new 
            {
                Property = n.Key,
                Message = n.Message
            });
            
            executedContext.Result = new BadRequestObjectResult(new { errors = notifications });
        }
    }
}
