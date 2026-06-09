using Application.Interfaces;
using Application.Notifications;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Services.Strategies;

public class ForeignKeyViolationStrategy : IPersistenceExceptionStrategy
{
    public bool CanHandle(Exception ex)
    {
        return ex is DbUpdateException dbEx && dbEx.InnerException is PostgresException { SqlState: "23503" };
    }

    public ErrorDetail Translate(Exception ex)
    {
        return new ErrorDetail("Database", "O registro não pode ser processado devido a uma dependência inexistente.");
    }
}
