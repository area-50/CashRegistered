using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class BankStatementImport : BaseEntity
{
    public int FinancialAccountId { get; private set; }
    public string FileName { get; private set; } = null!;
    public DateTime ImportDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int CreatedByUserId { get; private set; }

    public FinancialAccount FinancialAccount { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    private readonly List<BankStatementItem> _items = new();
    public IReadOnlyCollection<BankStatementItem> Items => _items.AsReadOnly();

    protected BankStatementImport() { }

    public BankStatementImport(
        int financialAccountId,
        string fileName,
        DateTime importDate,
        DateTime startDate,
        DateTime endDate,
        int createdByUserId)
    {
        FinancialAccountId = financialAccountId;
        FileName = fileName;
        ImportDate = importDate;
        StartDate = startDate;
        EndDate = endDate;
        CreatedByUserId = createdByUserId;

        Validate();
    }

    public void AddItem(BankStatementItem? item)
    {
        if (item == null) return;
        _items.Add(item);
    }

    private void Validate()
    {
        ClearNotifications();

        if (FinancialAccountId <= 0)
            AddNotification("ContaBancaria", "A conta bancária associada é obrigatória.");

        if (string.IsNullOrWhiteSpace(FileName))
            AddNotification("NomeArquivo", "O nome do arquivo de extrato é obrigatório.");
        else if (FileName.Length > 255)
            AddNotification("NomeArquivo", "O nome do arquivo de extrato não pode exceder 255 caracteres.");

        if (CreatedByUserId <= 0)
            AddNotification("UsuarioImportador", "O usuário importador é obrigatório.");
    }
}
