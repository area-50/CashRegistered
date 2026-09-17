using Domain.Shared.ValueObjects;
using Shared.Identity.Request;

namespace Shared.Identity.Response;

public class GetCustomerByIdResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public PersonResponse? Person { get; set; }
}
