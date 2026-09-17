using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Shared.DTOs;
using Domain.Shared.Response;

namespace Domain.Identity.Repositories;

public interface ISupplierRepository
{
    Task CreateAsync(Supplier entity);
    Task<Supplier?> GetByIdAsync(int id);
    Task<IEnumerable<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate);
    void Update(Supplier entity);
    void Delete(Supplier entity);
    Task<PagedResponse<Supplier>> SearchAsync(SearchSupplierRequest request);
}
