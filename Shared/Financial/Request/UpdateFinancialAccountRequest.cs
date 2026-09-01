using Domain.Financial.Enums;

namespace Shared.Financial.Request;

public class UpdateFinancialAccountRequest
{
    public string Name { get; set; } = null!;
    
    public FinancialAccountType AccountType { get; set; }
    
    public int ChartOfAccountsId { get; set; }
    
    public string? BankCode { get; set; }
    
    public string? BankName { get; set; }
    
    public string? AgencyNumber { get; set; }
    
    public string? AccountNumber { get; set; }
    
    public string Currency { get; set; } = "BRL";
    
    public bool IsActive { get; set; } = true;
}
