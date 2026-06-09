using Shared.Abstractions;
using Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Decorators;

public class UnitOfWorkDecorator(
    IUnitOfWork inner,
    PersistenceExceptionHandler exceptionHandler
) : IUnitOfWork
{
    public async Task<bool> CommitAsync()
    {
        try
        {
            return await inner.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            exceptionHandler.Handle(ex);
            return false;
        }
        catch (Exception ex)
        {
            exceptionHandler.Handle(ex);
            return false;
        }
    }
}
