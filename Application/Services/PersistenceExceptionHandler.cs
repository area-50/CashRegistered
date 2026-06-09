using Application.Interfaces;
using Application.Notifications;
using Shared.Notifications;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PersistenceExceptionHandler(
    IEnumerable<IPersistenceExceptionStrategy> strategies,
    NotificationContext notificationContext,
    ILogger<PersistenceExceptionHandler> logger
)
{
    public void Handle(Exception ex)
    {
        var strategy = strategies.FirstOrDefault(s => s.CanHandle(ex));

        if (strategy != null)
        {
            var error = strategy.Translate(ex);
            notificationContext.AddNotification(error.Key, error.Message);
        }
        else
        {
            logger.LogError(ex, "Erro de persistência não mapeado detectado.");
            notificationContext.AddNotification("Database", "Ocorreu um erro inesperado ao processar os dados.");
        }
    }
}
