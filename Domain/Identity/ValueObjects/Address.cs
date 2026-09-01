using Domain.Identity.Enums;
using Domain.Shared.Abstractions;

namespace Domain.Identity.ValueObjects;

public class Address : BaseEntity
{
    protected Address() { }

    public Address(string zipCode,
        string street,
        string number,
        string complement,
        string district,
        string city,
        string state,
        AddressType type,
        bool isDefault
    )
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Complement = complement;
        District = district;
        City = city;
        State = state;
        Type = type;
        IsDefault = isDefault;
    }

    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string Complement { get; private set; }
    public string District { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    
    public AddressType Type { get; private set; }
    public bool IsDefault { get; private set; }

    public void MarkAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;
}