using Domain.Shared.ValueObjects;

namespace Shared.Identity.Response;

public class GetAllPeopleResponse
{
    public int Id { get; set; }

    public Name  Name { get; set; }

    public string TaxId { get; set; }
}