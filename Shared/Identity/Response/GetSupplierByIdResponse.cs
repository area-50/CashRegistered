using Domain.Shared.ValueObjects;
using Shared.Identity.Request;

namespace Shared.Identity.Response;

public class GetSupplierByIdResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
    public PersonResponse? Person { get; set; }
}

public class PersonResponse
{
    public string PersonType { get; set; } = null!;
    public string Birthdate { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? TradeName { get; set; }
    public string? StateRegistration { get; set; }
    public string? MunicipalRegistration { get; set; }
    public string? CellPhone { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
}
