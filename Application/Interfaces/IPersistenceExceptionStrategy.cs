using Application.Notifications;

namespace Application.Interfaces;

public interface IPersistenceExceptionStrategy
{
    bool CanHandle(Exception ex);

    ErrorDetail Translate(Exception ex);
}
