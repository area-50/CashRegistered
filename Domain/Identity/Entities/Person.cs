using Domain.Identity.Enums;
using Domain.Identity.ValueObjects;
using Shared.Abstractions;
using Shared.Notifications;
using Shared.ValueObjects;
using Flunt.Notifications;
using Flunt.Validations;

namespace Domain.Identity.Entities;

public class Person : BaseEntity
{
    public Person(
        PersonType personType,
        string firstName,
        string lastName,
        string taxId,
        DateTime birthdate,
        string email,
        string? tradeName = null,
        string? stateRegistration = null,
        string? municipalRegistration = null,
        string? cellPhone = null,
        string? phone = null,
        string? gender = null
    )
    {
        PersonType = personType;
        Name = new Name(firstName, lastName);
        TaxId = taxId;
        Birthdate = birthdate;
        Email = email;
        TradeName = tradeName;
        StateRegistration = stateRegistration;
        MunicipalRegistration = municipalRegistration;
        CellPhone = cellPhone ?? string.Empty;
        Phone = phone ?? string.Empty;
        Gender = Enum.TryParse(gender, out Gender result) ? result : Enums.Gender.Other;

        Validate();
    }

    protected Person() { }

    public PersonType PersonType { get; set; }
    public Name Name { get; set; }
    public string TaxId { get; set; } // CPF or CNPJ
    public string? TradeName { get; set; } // Nome Fantasia
    public string? StateRegistration { get; set; } // IE
    public string? MunicipalRegistration { get; set; } // IM
    public DateTime Birthdate { get; set; }
    public string Email { get; set; }
    public string CellPhone { get; set; }
    public string Phone { get; set; }
    public Gender Gender { get; set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    private void Validate()
    {
        var contract = new Contract<Notification>()
            .Requires()
            .IsNotNullOrEmpty(TaxId, "CPF/CNPJ", "O CPF/CNPJ é obrigatório.")
            .IsEmail(Email, "E-mail", "E-mail inválido.");
        
        if (PersonType == PersonType.Legal)
        {
            contract.IsNotNullOrEmpty(
                TradeName, "Nome Fantasia",
                "O Nome Fantasia é obrigatório para Pessoa Jurídica."
            );
        }
        AddNotifications(contract.Notifications);
    }

    public void AddAddress(Address address)
    {
        _addresses.Add(address);
        RegisterUpdate();
    }

    public void ValidateUniquePerson(bool taxIdExists)
    {
        if (taxIdExists)
            AddNotification("CPF/CNPJ", "Já existe uma pessoa cadastrada com este CPF/CNPJ.");
    }

    public void Update(
        PersonType personType,
        string firstName,
        string lastName,
        string taxId,
        DateTime birthdate,
        string email,
        string? tradeName = null,
        string? stateRegistration = null,
        string? municipalRegistration = null,
        string? cellPhone = null,
        string? phone = null,
        string? gender = null
    )
    {
        PersonType = personType;
        Name = new Name(firstName, lastName);
        TaxId = taxId;
        Birthdate = birthdate;
        Email = email;
        TradeName = tradeName;
        StateRegistration = stateRegistration;
        MunicipalRegistration = municipalRegistration;
        CellPhone = cellPhone ?? string.Empty;
        Phone = phone ?? string.Empty;
        Gender = Enum.TryParse(gender, out Gender result) ? result : Enums.Gender.Other;

        ClearNotifications(); // Limpar validações antigas do Flunt
        Validate();
        RegisterUpdate();
    }

    public static void ValidatePersonExists(Person? targetPerson, NotificationContext notificationContext)
    {
        if (targetPerson == null)
            notificationContext.AddNotification("Pessoa", "A pessoa não existe.");
    }
}
