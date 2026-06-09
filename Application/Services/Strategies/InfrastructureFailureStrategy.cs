using Application.Interfaces;
using Application.Notifications;
using Microsoft.Extensions.Logging;

namespace Application.Services.Strategies;

public class InfrastructureFailureStrategy(ILogger<InfrastructureFailureStrategy> logger) : IPersistenceExceptionStrategy
{
    public bool CanHandle(Exception ex) => true;

    public ErrorDetail Translate(Exception ex)
    {
        logger.LogError(ex, "Erro crítico de infraestrutura detectado.");
        return new ErrorDetail("Database", "Ocorreu um erro interno ao processar a operação de banco de dados.");
    }
}
