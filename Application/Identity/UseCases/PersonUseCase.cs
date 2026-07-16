using Application.Identity.Interfaces;
using Domain.Identity.Entities;
using Domain.Identity.Enums;
using Domain.Identity.Repositories;
using Shared.Abstractions;
using Shared.Identity.Request;
using Shared.Identity.Response;
using Shared.Notifications;
using Shared.Response;
using Shared.Validations;

namespace Application.Identity.UseCases;

public class PersonUseCase(
    IPersonRepository repository,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
    ) : GeneralValidator, IPersonUseCase
{
    public async Task<CreateResponse> CreatePerson(CreatePersonRequest request)
    {
        var personType = Enum.IsDefined(typeof(PersonType), request.PersonType) 
            ? (PersonType)request.PersonType 
            : PersonType.Physical;

        var person = new Person(
            personType,
            request.FirstName,
            request.LastName,
            request.TaxId,
            request.Birthdate,
            request.Email,
            request.TradeName,
            request.StateRegistration,
            request.MunicipalRegistration,
            request.CellPhone,
            request.Phone,
            request.Gender
        );

        if (person.IsInvalid)
        {
            notificationContext.AddNotifications(person.Notifications);
            return new CreateResponse
            {
                Id = 0
            };
        }

        await repository.CreateAsync(person);
        await unitOfWork.CommitAsync();

        return new CreateResponse
        {
            Id = person.Id
        };
    }

    public Task<Person?> GetPersonByEmail(string email)
    {
        return repository.GetPersonByEmail(email);
    }

    public Task<Person?> GetPersonByTaxId(string taxId)
    {
        return repository.GetPersonByTaxId(taxId);
    }

    public async Task<IEnumerable<GetAllPeopleResponse>> GetAllPeople()
    {
        var response = await repository.FindAsync(p => true);
        return response.Select(person => new GetAllPeopleResponse
        {
            Id = person.Id,
            Name = person.Name,
            TaxId = person.TaxId
        });
    }

    public Task<Person?> GetPersonById(int id)
    {
        return repository.GetByIdAsync(id);
    }
}
