using Domain.Financial.Enums;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class FinancialAccount : BaseEntity
{
    public string Name { get; private set; } = null!;
    public FinancialAccountType AccountType { get; private set; }
    public string? BankCode { get; private set; }
    public string? BankName { get; private set; }
    public string? AgencyNumber { get; private set; }
    public string? AccountNumber { get; private set; }
    public int ChartOfAccountsId { get; private set; }
    public decimal InitialBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public string Currency { get; private set; } = "BRL";

    public ChartOfAccounts ChartOfAccounts { get; private set; } = null!;

    protected FinancialAccount() { }

    public FinancialAccount(
        string name,
        FinancialAccountType accountType,
        int chartOfAccountsId,
        decimal initialBalance = 0,
        string? bankCode = null,
        string? bankName = null,
        string? agencyNumber = null,
        string? accountNumber = null,
        string currency = "BRL")
    {
        Name = name;
        AccountType = accountType;
        ChartOfAccountsId = chartOfAccountsId;
        InitialBalance = initialBalance;
        CurrentBalance = initialBalance;
        BankCode = bankCode;
        BankName = bankName;
        AgencyNumber = agencyNumber;
        AccountNumber = accountNumber;
        Currency = string.IsNullOrWhiteSpace(currency) ? "BRL" : currency;

        Validate();
    }

    public void Update(
        string name,
        FinancialAccountType accountType,
        int chartOfAccountsId,
        string? bankCode = null,
        string? bankName = null,
        string? agencyNumber = null,
        string? accountNumber = null,
        string currency = "BRL")
    {
        Name = name;
        AccountType = accountType;
        ChartOfAccountsId = chartOfAccountsId;
        BankCode = bankCode;
        BankName = bankName;
        AgencyNumber = agencyNumber;
        AccountNumber = accountNumber;
        Currency = string.IsNullOrWhiteSpace(currency) ? "BRL" : currency;

        RegisterUpdate();
        Validate();
    }

    public void CreditBalance(decimal amount)
    {
        if (amount < 0)
            AddNotification("Valor", "O valor a creditar não pode ser negativo.");

        CurrentBalance += amount;
        RegisterUpdate();
    }

    public void DebitBalance(decimal amount)
    {
        if (amount < 0)
            AddNotification("Valor", "O valor a debitar não pode ser negativo.");

        CurrentBalance -= amount;
        RegisterUpdate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(Name))
            AddNotification("Nome", "O nome da conta financeira é obrigatório.");
        else if (Name.Length > 100)
            AddNotification("Nome", "O nome da conta financeira não pode exceder 100 caracteres.");

        if (ChartOfAccountsId <= 0)
            AddNotification("PlanoDeContas", "A conta do plano de contas associada é obrigatória.");

        if (!string.IsNullOrEmpty(BankCode) && BankCode.Length > 20)
            AddNotification("CodigoBanco", "O código do banco não pode exceder 20 caracteres.");

        if (!string.IsNullOrEmpty(BankName) && BankName.Length > 100)
            AddNotification("NomeBanco", "O nome do banco não pode exceder 100 caracteres.");

        if (!string.IsNullOrEmpty(AgencyNumber) && AgencyNumber.Length > 20)
            AddNotification("Agencia", "O número da agência não pode exceder 20 caracteres.");

        if (!string.IsNullOrEmpty(AccountNumber) && AccountNumber.Length > 30)
            AddNotification("ContaBancaria", "O número da conta bancária não pode exceder 30 caracteres.");

        if (Currency.Length > 3)
            AddNotification("Moeda", "A moeda não pode exceder 3 caracteres.");
    }
}
