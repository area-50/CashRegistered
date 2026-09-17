namespace Shared.Identity.Request;

public class CreateCustomerRequest
{
    public int? PersonId { get; set; }
    public CreatePersonRequest? Person { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
}
