using Domain.Financial.Enums;

namespace Shared.Financial.Request;

public class UpdateChartOfAccountsRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsSynthetic { get; set; }
    public bool AllowPosting { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
