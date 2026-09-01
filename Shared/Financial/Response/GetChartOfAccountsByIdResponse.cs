using Domain.Financial.Enums;

namespace Shared.Financial.Response;

public class GetChartOfAccountsByIdResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsSynthetic { get; set; }
    public bool AllowPosting { get; set; }
    public bool IsActive { get; set; }
}
