namespace Domain.Shared.Abstractions;

public interface IUnitOfWork
{
    Task<bool> CommitAsync();
}