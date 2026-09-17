using Application.Identity.Interfaces;
using Domain.Identity.Entities;
using Domain.Identity.Repositories;
using Domain.Shared.Abstractions;
using Domain.Shared.DTOs;
using Shared.Identity.Request;
using Shared.Identity.Response;

namespace Application.Identity.UseCases;

public class CustomerUseCase(
    ICustomerRepository repository,
    IPersonUseCase personUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : ICustomerUseCase
{
    public async Task<CreateResponse> CreateCustomer(CreateCustomerRequest request)
    {
        int personId;

        if (request.PersonId > 0)
        {
            var person = await personUseCase.GetPersonById(request.PersonId.Value);
            if (person == null)
            {
                notificationContext.AddNotification("Person", "A pessoa informada não existe.");
                return new CreateResponse();
            }
            personId = request.PersonId.Value;
        }
        else
        {
            if (request.Person == null)
            {
                notificationContext.AddNotification(
                    "Person", "Os dados da pessoa são obrigatórios para um novo cadastro."
                );
                return new CreateResponse();
            }

            var personResponse = await personUseCase.CreatePerson(request.Person);
            if (personResponse.Id == 0)
            {
                return new CreateResponse();
            }
            personId = personResponse.Id;
        }

        var customer = new Customer(personId, request.CreditLimit, request.Notes);

        if (customer.IsInvalid)
        {
            notificationContext.AddNotifications(customer.Notifications);
            return new CreateResponse();
        }

        await repository.CreateAsync(customer);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = customer.Id };
    }

    public async Task<GetCustomerByIdResponse?> GetCustomerById(int id)
    {
        var customer = await repository.GetByIdAsync(id);
        if (Customer.NotExists(customer, notificationContext)) return new GetCustomerByIdResponse();

        return new GetCustomerByIdResponse
        {
            Id = customer!.Id,
            PersonId = customer.PersonId,
            Name = customer.Person.Name,
            TaxId = customer.Person.TaxId,
            CreditLimit = customer.CreditLimit,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            Person = new PersonResponse
            {
                PersonType = customer.Person.PersonType.ToString(),
                Birthdate = customer.Person.Birthdate.ToString("yyyy-MM-dd"),
                Email = customer.Person.Email,
                TradeName = customer.Person.TradeName,
                StateRegistration = customer.Person.StateRegistration,
                MunicipalRegistration = customer.Person.MunicipalRegistration,
                CellPhone = customer.Person.CellPhone,
                Phone = customer.Person.Phone,
                Gender = customer.Person.Gender.ToString()
            }
        };
    }

    public async Task<PagedResponse<GetSearchCustomerResponse>> SearchCustomers(Shared.Identity.Request.SearchCustomerRequest request)
    {
        var domainRequest = new Domain.Shared.DTOs.SearchCustomerRequest
        {
            Name = request.Name,
            TaxId = request.TaxId,
            IsActive = request.IsActive,
            Page = request.Page,
            PageSize = request.PageSize
        };
        var pagedCustomers = await repository.SearchAsync(domainRequest);

        return new PagedResponse<GetSearchCustomerResponse>
        {
            Items = pagedCustomers.Items.Select(c => new GetSearchCustomerResponse
            {
                Id = c.Id,
                PersonId = c.PersonId,
                Name = c.Person.Name,
                TaxId = c.Person.TaxId,
                CreditLimit = c.CreditLimit,
                IsActive = c.IsActive
            }),
            TotalCount = pagedCustomers.TotalCount,
            Page = pagedCustomers.Page,
            PageSize = pagedCustomers.PageSize
        };
    }

    public async Task UpdateCustomer(int id, UpdateCustomerRequest request)
    {
        var customer = await repository.GetByIdAsync(id);
        if (Customer.NotExists(customer, notificationContext)) return;

        if (request.IsActive)
            customer!.Activate();
        else
            customer!.Deactivate();

        customer!.Update(request.CreditLimit, request.Notes);

        if (customer.IsInvalid)
        {
            notificationContext.AddNotifications(customer.Notifications);
            return;
        }

        if (request.Person != null)
        {
            await personUseCase.UpdatePerson(customer.PersonId, request.Person);
        }

        repository.Update(customer);
        await unitOfWork.CommitAsync();
    }

    public async Task DeactivateCustomer(int id)
    {
        var customer = await repository.GetByIdAsync(id);
        if (Customer.NotExists(customer, notificationContext)) return;

        customer!.Deactivate();

        repository.Update(customer);
        await unitOfWork.CommitAsync();
    }
}
