using Domain.Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetSearchCustomerResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; }
}
