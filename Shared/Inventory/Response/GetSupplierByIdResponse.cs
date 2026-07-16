using Shared.ValueObjects;

namespace Shared.Inventory.Response;

public class GetSupplierByIdResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}
