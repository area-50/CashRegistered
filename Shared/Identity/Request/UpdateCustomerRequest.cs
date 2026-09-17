namespace Shared.Identity.Request;

public class UpdateCustomerRequest
{
    public int Id { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public UpdatePersonRequest? Person { get; set; }
}
