using Domain.Financial.Entities;
using Domain.Financial.Enums;
using Xunit;

namespace Tests.Financial.Domain.Entities;

public class JournalEntryTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateInstanceAndSetStatusDraft()
    {
        // Act
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento de Compra", "AccountsPayable", createdByUserId: 1);

        // Assert
        Assert.Equal("Lançamento de Compra", entry.Description);
        Assert.Equal("AccountsPayable", entry.SourceModule);
        Assert.Equal(1, entry.CreatedByUserId);
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.Equal(0, entry.TotalDebit);
        Assert.Equal(0, entry.TotalCredit);
        Assert.False(entry.IsInvalid);
        Assert.Empty(entry.Notifications);
    }

    [Fact]
    public void Constructor_InvalidData_ShouldAddNotifications()
    {
        // Act
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "", "", createdByUserId: 0);

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Descricao" && n.Message == "A descrição do lançamento contábil é obrigatória.");
        Assert.Contains(entry.Notifications, n => n.Key == "ModuloOrigem" && n.Message == "O módulo de origem é obrigatório.");
        Assert.Contains(entry.Notifications, n => n.Key == "UsuarioCriador" && n.Message == "O usuário criador do lançamento é obrigatório.");
    }

    [Fact]
    public void Constructor_ExceedingLengths_ShouldAddNotifications()
    {
        // Arrange
        var longDesc = new string('D', 256);
        var longModule = new string('M', 51);
        var longDoc = new string('R', 101);

        // Act
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, longDesc, longModule, createdByUserId: 1, referenceDocument: longDoc);

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Descricao");
        Assert.Contains(entry.Notifications, n => n.Key == "ModuloOrigem");
        Assert.Contains(entry.Notifications, n => n.Key == "DocumentoReferencia");
    }

    [Fact]
    public void Post_UnbalancedDebitsAndCredits_ShouldAddNotificationAndNotPost()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento de Compra", "AccountsPayable", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 100.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 50.00m));

        // Act
        entry.Post();

        // Assert
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "PartidasDobradas" && n.Message.Contains("desbalanceado"));
    }

    [Fact]
    public void Post_NoItems_ShouldAddNotificationAndNotPost()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento Sem Itens", "Manual", createdByUserId: 1);

        // Act
        entry.Post();

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Items" && n.Message.Contains("ao menos uma partida"));
    }

    [Fact]
    public void Post_BalancedDebitsAndCredits_ShouldSetStatusToPosted()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento de Compra", "AccountsPayable", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 100.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 100.00m));

        // Act
        entry.Post();

        // Assert
        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        Assert.False(entry.IsInvalid);
        Assert.Equal(100.00m, entry.TotalDebit);
        Assert.Equal(100.00m, entry.TotalCredit);
    }

    [Fact]
    public void Post_AlreadyPostedEntry_ShouldAddNotification()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento", "Manual", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 100.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 100.00m));
        entry.Post();

        // Act
        entry.Post();

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Status" && n.Message.Contains("já está escriturado"));
    }

    [Fact]
    public void Reverse_PostedEntry_ShouldCreateOppositeReversalEntryAndSetStatusReversed()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento Original", "Manual", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 250.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 250.00m));
        entry.Post();

        // Act
        var reversal = entry.Reverse(createdByUserId: 2, reversalReason: "Lançamento em duplicidade");

        // Assert
        Assert.Equal(JournalEntryStatus.Reversed, entry.Status);
        Assert.NotNull(reversal);
        Assert.Equal(JournalEntryStatus.Posted, reversal.Status);
        Assert.Equal(250.00m, reversal.TotalDebit);
        Assert.Equal(250.00m, reversal.TotalCredit);
        Assert.Equal(2, reversal.Items.Count);
        Assert.Contains(reversal.Items, i => i.AccountId == 10 && i.EntryType == JournalEntryType.Credit && i.Amount == 250.00m);
    }

    [Fact]
    public void Reverse_DraftEntry_ShouldAddNotificationAndReturnNull()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Rascunho", "Manual", createdByUserId: 1);

        // Act
        var reversal = entry.Reverse(createdByUserId: 2, reversalReason: "Motivo");

        // Assert
        Assert.Null(reversal);
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Status" && n.Message.Contains("Apenas lançamentos escriturados"));
    }

    [Fact]
    public void AddItem_ToPostedEntry_ShouldAddNotificationAndReject()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento", "Manual", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 100.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 100.00m));
        entry.Post();

        // Act
        entry.AddItem(new JournalEntryItem(accountId: 30, entryType: JournalEntryType.Debit, amount: 50.00m));

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Status" && n.Message.Contains("escriturado"));
    }

    [Fact]
    public void Cancel_DraftEntry_ShouldSetStatusCanceled()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Rascunho", "Manual", createdByUserId: 1);

        // Act
        entry.Cancel();

        // Assert
        Assert.Equal(JournalEntryStatus.Canceled, entry.Status);
        Assert.False(entry.IsInvalid);
    }

    [Fact]
    public void Cancel_PostedEntry_ShouldAddNotification()
    {
        // Arrange
        var entry = new JournalEntry(DateTime.UtcNow, DateTime.UtcNow, "Lançamento", "Manual", createdByUserId: 1);
        entry.AddItem(new JournalEntryItem(accountId: 10, entryType: JournalEntryType.Debit, amount: 100.00m));
        entry.AddItem(new JournalEntryItem(accountId: 20, entryType: JournalEntryType.Credit, amount: 100.00m));
        entry.Post();

        // Act
        entry.Cancel();

        // Assert
        Assert.True(entry.IsInvalid);
        Assert.Contains(entry.Notifications, n => n.Key == "Status" && n.Message.Contains("Lançamentos escriturados não podem ser cancelados diretamente"));
    }
}
