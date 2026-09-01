using Domain.Identity.Entities;
using Domain.Financial.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Identity.Repositories;

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetPersonByEmail(string email);
    
    Task<Person?> GetPersonByTaxId(string taxId);
}