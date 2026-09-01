using Domain.Financial.Enums;

namespace Shared.Financial.Response;

public class GetSearchFinancialAccountResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public FinancialAccountType AccountType { get; set; }
    public string AccountTypeName { get; set; } = null!;
    public int ChartOfAccountsId { get; set; }
    public string ChartOfAccountsName { get; set; } = null!;
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? AgencyNumber { get; set; }
    public string? AccountNumber { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "BRL";
    public bool IsActive { get; set; }
}
