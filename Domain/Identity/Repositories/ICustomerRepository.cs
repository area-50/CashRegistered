using System.Linq.Expressions;
using Domain.Identity.Entities;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;

namespace Domain.Identity.Repositories;

public interface ICustomerRepository
{
    Task CreateAsync(Customer entity);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByPersonIdAsync(int personId);
    Task<IEnumerable<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate);
    void Update(Customer entity);
    void Delete(Customer entity);
    Task<PagedResponse<Customer>> SearchAsync(SearchCustomerRequest request);
}
