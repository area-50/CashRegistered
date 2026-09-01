using Domain.Financial.Enums;
using Domain.Identity.Entities;
using Domain.Shared.Abstractions;

namespace Domain.Financial.Entities;

public class JournalEntry : BaseEntity
{
    public DateTime EntryDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; } = null!;
    public string SourceModule { get; private set; } = null!;
    public string? ReferenceDocument { get; private set; }
    public decimal TotalDebit { get; private set; }
    public decimal TotalCredit { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public int? ReversedJournalEntryId { get; private set; }
    public int CreatedByUserId { get; private set; }

    public JournalEntry? ReversedJournalEntry { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    private readonly List<JournalEntryItem> _items = new();
    public IReadOnlyCollection<JournalEntryItem> Items => _items.AsReadOnly();

    protected JournalEntry() { }

    public JournalEntry(
        DateTime entryDate,
        DateTime postingDate,
        string description,
        string sourceModule,
        int createdByUserId,
        string? referenceDocument = null)
    {
        EntryDate = entryDate;
        PostingDate = postingDate;
        Description = description;
        SourceModule = sourceModule;
        CreatedByUserId = createdByUserId;
        ReferenceDocument = referenceDocument;
        TotalDebit = 0;
        TotalCredit = 0;
        Status = JournalEntryStatus.Draft;

        Validate();
    }

    public void AddItem(JournalEntryItem item)
    {
        if (Status == JournalEntryStatus.Posted)
        {
            AddNotification("Status", "Não é possível adicionar itens a um lançamento já escriturado (Posted).");
            return;
        }

        if (item == null) return;

        _items.Add(item);
        RecalculateTotals();
    }

    public void Post()
    {
        if (Status == JournalEntryStatus.Posted)
        {
            AddNotification("Status", "Este lançamento já está escriturado.");
            return;
        }

        if (!_items.Any())
        {
            AddNotification("Items", "Um lançamento contábil deve possuir ao menos uma partida.");
            return;
        }

        RecalculateTotals();

        if (Math.Abs(TotalDebit - TotalCredit) > 0.001m)
        {
            AddNotification("PartidasDobradas", $"O lançamento contábil está desbalanceado. Soma dos Débitos: R$ {TotalDebit:N2}, Soma dos Créditos: R$ {TotalCredit:N2}. A diferença deve ser zero.");
            return;
        }

        Status = JournalEntryStatus.Posted;
        RegisterUpdate();
    }

    public JournalEntry Reverse(int createdByUserId, string reversalReason)
    {
        if (Status != JournalEntryStatus.Posted)
        {
            AddNotification("Status", "Apenas lançamentos escriturados (Posted) podem ser estornados.");
            return null!;
        }

        var reversalEntry = new JournalEntry(
            DateTime.UtcNow,
            DateTime.UtcNow,
            $"ESTORNO: {Description} - Motivo: {reversalReason}",
            SourceModule,
            createdByUserId,
            ReferenceDocument);

        foreach (var item in _items)
        {
            var oppositeType = item.EntryType == JournalEntryType.Debit ? JournalEntryType.Credit : JournalEntryType.Debit;
            reversalEntry.AddItem(new JournalEntryItem(item.AccountId, oppositeType, item.Amount, item.CostCenterId, $"Estorno de item id {item.Id}"));
        }

        reversalEntry.Post();

        Status = JournalEntryStatus.Reversed;
        ReversedJournalEntryId = reversalEntry.Id;
        RegisterUpdate();

        return reversalEntry;
    }

    public void Cancel()
    {
        if (Status == JournalEntryStatus.Posted)
        {
            AddNotification("Status", "Lançamentos escriturados não podem ser cancelados diretamente, apenas estornados.");
            return;
        }

        Status = JournalEntryStatus.Canceled;
        RegisterUpdate();
    }

    public void RecalculateTotals()
    {
        TotalDebit = _items.Where(i => i.EntryType == JournalEntryType.Debit).Sum(i => i.Amount);
        TotalCredit = _items.Where(i => i.EntryType == JournalEntryType.Credit).Sum(i => i.Amount);
    }

    private void Validate()
    {
        ClearNotifications();

        if (string.IsNullOrWhiteSpace(Description))
            AddNotification("Descricao", "A descrição do lançamento contábil é obrigatória.");
        else if (Description.Length > 255)
            AddNotification("Descricao", "A descrição não pode exceder 255 caracteres.");

        if (string.IsNullOrWhiteSpace(SourceModule))
            AddNotification("ModuloOrigem", "O módulo de origem é obrigatório.");
        else if (SourceModule.Length > 50)
            AddNotification("ModuloOrigem", "O módulo de origem não pode exceder 50 caracteres.");

        if (CreatedByUserId <= 0)
            AddNotification("UsuarioCriador", "O usuário criador do lançamento é obrigatório.");

        if (!string.IsNullOrEmpty(ReferenceDocument) && ReferenceDocument.Length > 100)
            AddNotification("DocumentoReferencia", "O documento de referência não pode exceder 100 caracteres.");
    }
}
