using Application.Identity.Interfaces;
using Application.Inventory.Interfaces;
using Domain.Inventory.Entities;
using Domain.Inventory.Interfaces;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Inventory.Response;
using Shared.Notifications;
using Shared.Response;

namespace Application.Inventory.UseCases;

public class SupplierUseCase(
    ISupplierRepository repository,
    IPersonUseCase personUseCase,
    IUnitOfWork unitOfWork,
    NotificationContext notificationContext
) : ISupplierUseCase
{
    public async Task<CreateResponse> CreateSupplier(CreateSupplierRequest request)
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
            personId = person.Id;
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

        var supplier = new Supplier(personId);
        
        await repository.CreateAsync(supplier);
        await unitOfWork.CommitAsync();

        return new CreateResponse { Id = supplier.Id };
    }

    public async Task<GetSupplierByIdResponse?> GetSupplierById(int id)
    {
        var supplier = await repository.GetByIdAsync(id);
        if (supplier == null)
        {
            return null;
        }

        return new GetSupplierByIdResponse
        {
            Id = supplier.Id,
            PersonId = supplier.PersonId,
            Name = supplier.Person.Name,
            TaxId = supplier.Person.TaxId,
            IsActive = supplier.IsActive,
            Person = new PersonDto
            {
                PersonType = supplier.Person.PersonType.ToString(),
                Birthdate = supplier.Person.Birthdate.ToString("yyyy-MM-dd"),
                Email = supplier.Person.Email,
                TradeName = supplier.Person.TradeName,
                StateRegistration = supplier.Person.StateRegistration,
                MunicipalRegistration = supplier.Person.MunicipalRegistration,
                CellPhone = supplier.Person.CellPhone,
                Phone = supplier.Person.Phone,
                Gender = supplier.Person.Gender.ToString()
            }
        };
    }

    public async Task<PagedResponse<GetSearchSupplierResponse>> SearchSuppliers(SearchSupplierRequest request)
    {
        var pagedSuppliers = await repository.SearchAsync(request);

        return new PagedResponse<GetSearchSupplierResponse>
        {
            Items = pagedSuppliers.Items.Select(s => new GetSearchSupplierResponse
            {
                Id = s.Id,
                Name = s.Person.Name,
                TaxId = s.Person.TaxId,
                IsActive = s.IsActive
            }),
            TotalCount = pagedSuppliers.TotalCount,
            Page = pagedSuppliers.Page,
            PageSize = pagedSuppliers.PageSize
        };
    }

    public async Task UpdateSupplier(int id, UpdateSupplierRequest request)
    {
        var supplier = await repository.GetByIdAsync(id);
        if (supplier == null)
        {
            notificationContext.AddNotification("Supplier", "O fornecedor não existe.");
            return;
        }

        if (request.IsActive)
            supplier.Activate();
        else
            supplier.Deactivate();

        if (request.Person != null)
        {
            await personUseCase.UpdatePerson(supplier.PersonId, request.Person);
        }

        repository.Update(supplier);
        await unitOfWork.CommitAsync();
    }

    public async Task DeactivateSupplier(int id)
    {
        var supplier = await repository.GetByIdAsync(id);
        if (supplier == null)
        {
            notificationContext.AddNotification("Supplier", "O fornecedor não existe.");
            return;
        }

        supplier.Deactivate();

        repository.Update(supplier);
        await unitOfWork.CommitAsync();
    }
}
