using Application.Interfaces;
using Application.Notifications;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Services.Strategies;

public class UniqueConstraintStrategy : IPersistenceExceptionStrategy
{
    public bool CanHandle(Exception ex)
    {
        return ex is DbUpdateException dbEx && dbEx.InnerException is PostgresException { SqlState: "23505" };
    }

    public ErrorDetail Translate(Exception ex)
    {
        var pgEx = (PostgresException)ex.InnerException!;
        
        return pgEx.ConstraintName switch
        {
            "UX_Product_Sku" => new ErrorDetail("Sku", "Já existe um produto com este SKU."),
            "UX_User_Email" => new ErrorDetail("Email", "Este e-mail já está em uso."),
            _ => new ErrorDetail("Database", "Este registro já existe no sistema.")
        };
    }
}
