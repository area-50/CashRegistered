using Domain.Financial.Enums;
using Domain.Shared.Abstractions;

namespace Domain.Shared.DTOs;

public class SearchFinancialAccountRequest : PagedRequest
{
    public string? Name { get; set; }
    public FinancialAccountType? AccountType { get; set; }
    public bool? IsActive { get; set; }
}
