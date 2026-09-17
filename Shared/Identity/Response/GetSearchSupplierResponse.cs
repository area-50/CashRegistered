using Domain.Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetSearchSupplierResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}
