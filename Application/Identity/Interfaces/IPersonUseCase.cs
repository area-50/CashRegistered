using Domain.Identity.Entities;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Shared.Response;

namespace Application.Identity.Interfaces;

public interface IPersonUseCase
{
    Task<CreateResponse> CreatePerson(CreatePersonRequest request);
    
    Task<Person?> GetPersonByEmail(string email);
    
    Task<Person?> GetPersonByTaxId(string taxId);

    Task<IEnumerable<GetAllPeopleResponse>> GetAllPeople();
}