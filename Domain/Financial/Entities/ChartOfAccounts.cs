using Domain.Financial.Enums;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class ChartOfAccounts : BaseEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public AccountNature Nature { get; private set; }
    public int? ParentAccountId { get; private set; }
    public bool IsSynthetic { get; private set; }
    public bool AllowPosting { get; private set; }

    public ChartOfAccounts? ParentAccount { get; private set; }
    
    private readonly List<ChartOfAccounts> _childAccounts = new();
    public IReadOnlyCollection<ChartOfAccounts> ChildAccounts => _childAccounts.AsReadOnly();

    protected ChartOfAccounts() { }

    public ChartOfAccounts(
        string code,
        string name,
        AccountType accountType,
        AccountNature nature,
        int? parentAccountId = null,
        bool isSynthetic = false,
        bool allowPosting = true)
    {
        Code = code;
        Name = name;
        AccountType = accountType;
        Nature = nature;
        ParentAccountId = parentAccountId;
        IsSynthetic = isSynthetic;
        AllowPosting = isSynthetic ? false : allowPosting;

        Validate();
    }

    public void Update(string code, string name, AccountType accountType, AccountNature nature, int? parentAccountId, bool isSynthetic, bool allowPosting)
    {
        Code = code;
        Name = name;
        AccountType = accountType;
        Nature = nature;
        ParentAccountId = parentAccountId;
        IsSynthetic = isSynthetic;
        AllowPosting = isSynthetic ? false : allowPosting;

        RegisterUpdate();
        Validate();
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(Code))
            AddNotification("Codigo", "O código do plano de contas é obrigatório.");
        else if (Code.Length > 50)
            AddNotification("Codigo", "O código do plano de contas não pode exceder 50 caracteres.");

        if (string.IsNullOrWhiteSpace(Name))
            AddNotification("Nome", "O nome da conta é obrigatório.");
        else if (Name.Length > 150)
            AddNotification("Nome", "O nome da conta não pode exceder 150 caracteres.");

        if (IsSynthetic && AllowPosting)
            AddNotification("PermiteLancamentos", "Contas sintéticas não podem permitir lançamentos diretos.");
    }
}
