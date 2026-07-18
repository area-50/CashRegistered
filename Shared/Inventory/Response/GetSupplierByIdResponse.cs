using Shared.ValueObjects;

namespace Shared.Inventory.Response;

public class GetSupplierByIdResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public PersonDto? Person { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}

public class PersonDto
{
    public string? PersonType { get; set; }
    public string? Birthdate { get; set; }
    public string? Email { get; set; }
    public string? TradeName { get; set; }
    public string? StateRegistration { get; set; }
    public string? MunicipalRegistration { get; set; }
    public string? CellPhone { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
}
