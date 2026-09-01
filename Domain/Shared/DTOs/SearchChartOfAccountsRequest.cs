using Domain.Financial.Enums;

namespace Domain.Shared.DTOs;

public class SearchChartOfAccountsRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public AccountType? AccountType { get; set; }
    public AccountNature? Nature { get; set; }
    public bool? IsSynthetic { get; set; }
    public bool? AllowPosting { get; set; }
    public bool? IsActive { get; set; }
}
