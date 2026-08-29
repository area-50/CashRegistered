using Domain.Financial.Enums;
using Shared.Abstractions;

namespace Domain.Financial.Entities;

public class JournalEntryItem : BaseEntity
{
    public int JournalEntryId { get; private set; }
    public int AccountId { get; private set; }
    public int? CostCenterId { get; private set; }
    public JournalEntryType EntryType { get; private set; }
    public decimal Amount { get; private set; }
    public string? Memo { get; private set; }

    public JournalEntry JournalEntry { get; private set; } = null!;
    public ChartOfAccounts Account { get; private set; } = null!;
    public CostCenter? CostCenter { get; private set; }

    protected JournalEntryItem() { }

    public JournalEntryItem(
        int accountId,
        JournalEntryType entryType,
        decimal amount,
        int? costCenterId = null,
        string? memo = null)
    {
        AccountId = accountId;
        EntryType = entryType;
        Amount = amount;
        CostCenterId = costCenterId;
        Memo = memo;

        Validate();
    }

    internal void AttachToJournalEntry(int journalEntryId)
    {
        JournalEntryId = journalEntryId;
    }

    private void Validate()
    {
        ClearNotifications();

        if (AccountId <= 0)
            AddNotification("ContaAnalitica", "A conta analítica do plano de contas é obrigatória.");

        if (Amount <= 0)
            AddNotification("ValorPartida", "O valor da partida contábil deve ser maior que zero.");

        if (!string.IsNullOrEmpty(Memo) && Memo.Length > 255)
            AddNotification("Observacao", "A observação do item não pode exceder 255 caracteres.");
    }
}
