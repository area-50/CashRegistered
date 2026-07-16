using Shared.ValueObjects;

namespace Shared.Inventory.Response;

public class GetSearchSupplierResponse
{
    public int Id { get; set; }
    public Name? Name { get; set; }
    public string? TaxId { get; set; }
    public bool IsActive { get; set; }
}
