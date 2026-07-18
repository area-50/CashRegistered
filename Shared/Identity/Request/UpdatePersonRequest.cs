namespace Shared.Identity.Request;

public class UpdatePersonRequest
{
    public string PersonType { get; set; } = "Physical"; // "Physical" or "Legal"
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string TaxId { get; set; } = null!; // CPF or CNPJ
    public string? TradeName { get; set; }
    public string? StateRegistration { get; set; }
    public string? MunicipalRegistration { get; set; }
    public DateTime Birthdate { get; set; }
    public string Email { get; set; } = null!;
    public string? CellPhone { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
}
